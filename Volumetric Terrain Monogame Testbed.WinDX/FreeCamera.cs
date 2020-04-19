using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// A generic, freely moveable camera.
    /// </summary>
    internal class FreeCamera
    {
        #region Fields
        /// <summary>
        /// The aspect ratio (width / height) of the field of view of this camera.
        /// </summary>
        private float aspectRatio;

        /// <summary>
        /// The direction in world space that is considered "up" by default.
        /// </summary>
        private static readonly Vector3 defaultUpDirection = Vector3.Up;

        /// <summary>
        /// The default viewing direction in world space.
        /// </summary>
        private static readonly Vector3 defaultViewingDirection = Vector3.Forward;

        /// <summary>
        /// The distance to the far clipping plane.
        /// </summary>
        private float farClippingPlaneDistance;

        /// <summary>
        /// The distance to the near clipping plane.
        /// </summary>
        private float nearClippingPlaneDistance;

        /// <summary>
        /// The position of this camera in world space;
        /// </summary>
        private Vector3 position;

        /// <summary>
        /// The projection matrix used by this camera.
        /// Used to move an object from camera space to projection/screen space.
        /// </summary>
        private Matrix projectionMatrix;

        /// <summary>
        /// Indicates if the projection matrix has to be recalculated.
        /// Used for lazy updates.
        /// </summary>
        private bool projectionMatrixDirty;

        /// <summary>
        /// The direction in world space that is considered "right" for the camera's current rotation.
        /// </summary>
        private Vector3 rightDirection;

        /// <summary>
        /// The direction in world space that is considered "up" for the camera's current rotation.
        /// </summary>
        private Vector3 upDirection;

        /// <summary>
        /// The vertical field of view (https://en.wikipedia.org/wiki/Field_of_view_in_video_games) of this camera noted in degree.
        /// </summary>
        private float verticalFieldOfViewDegree;

        /// <summary>
        /// The viewing direction of this camera in world space.
        /// </summary>
        private Vector3 viewDirection;

        /// <summary>
        /// The view frustrum of this camera in world space.
        /// </summary>
        private BoundingFrustum viewFrustrum;

        /// <summary>
        /// Indicates if the view frustrum has to be recalculated.
        /// Used for lazy updates.
        /// </summary>
        private bool viewFrustrumDirty;

        /// <summary>
        /// The view matrix of this camera.
        /// Used to move an object from world space to camera/view space.
        /// </summary>
        private Matrix viewMatrix;

        /// <summary>
        /// Indicates if the view frustum has to be recalculated.
        /// Used for lazy updates.
        /// </summary>
        private bool viewMatrixDirty;

        /// <summary>
        /// Describes the current rotation of this camera.
        /// </summary>
        private Quaternion rotation;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new FreeCamera instance using some default values.
        /// </summary>
        /// <remarks>
        /// Position will be set to the world space origin (0.0, 0.0, 0.0).
        /// Viewing direction will be set to (0.0, 0.0, -1.0) (= "into the screen").
        /// Up direction will be set to (0.0, 1.0, 0.0) (= "up as normally perceived").
        /// The vertical field of view will be set to 90°.
        /// The near clipping plane will be placed at a distance of 1.0.
        /// The far clipping plane will be placed at a distance of 10,000.0.
        /// </remarks>
        internal FreeCamera() : this(Vector3.Zero, FreeCamera.defaultViewingDirection, FreeCamera.defaultUpDirection, 90.0f, 1.0f, 10000.0f)
        {
        }

        /// <summary>
        /// Creates a new FreeCamera instance.
        /// </summary>
        /// <param name="position">The starting position of the camera in world space.</param>
        /// <param name="viewDirection">The initial viewing direction (not the target point!) of the camera in world space.</param>
        /// <param name="upDirection">The initial up direction of the camera.</param>
        /// <param name="verticalFieldOfView">The vertical field of view / the vertical aperture angle (https://en.wikipedia.org/wiki/Field_of_view_in_video_games) of the camera in degrees.</param>
        /// <param name="nearClippingPlaneDistance">The distance to the near clipping plane.</param>
        /// <param name="farClippingPlaneDistance">The distance to the far clipping plane.</param>
        internal FreeCamera(Vector3 position, Vector3 viewDirection, Vector3 upDirection, float verticalFieldOfView, float nearClippingPlaneDistance, float farClippingPlaneDistance)
        {
            // Default values.
            this.rotation = Quaternion.Identity;
            this.projectionMatrix = Matrix.Identity;
            this.projectionMatrixDirty = true;
            this.viewFrustrumDirty = true;
            this.viewMatrix = Matrix.Identity;
            this.viewMatrixDirty = true;

            // Check and transfer values from call.
            if (farClippingPlaneDistance <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(farClippingPlaneDistance), "The far clipping plane has to have a disctance > 0.0 to the camera.");
            }
            else
            {
                this.farClippingPlaneDistance = farClippingPlaneDistance;
            }

            if (nearClippingPlaneDistance <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(farClippingPlaneDistance), "The near clipping plane has to have a distance > 0.0 to the camera.");
            }
            else if (nearClippingPlaneDistance >= farClippingPlaneDistance)
            {
                throw new ArgumentException(nameof(nearClippingPlaneDistance) + " / " + nameof(farClippingPlaneDistance), 
                    "The far clipping plane has to be further away from the camera than the near clipping plane.");
            }
            else
            {
                this.nearClippingPlaneDistance = nearClippingPlaneDistance;
            }

            this.position = position;
            if (upDirection.Length() == 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(upDirection), "The up Direction has to be a direction. A vector of length 0.0 does not have a direction.");
            }
            else
            {
                this.upDirection = Vector3.Normalize(upDirection);
            }

            if (verticalFieldOfView <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalFieldOfView), "The field of view has to be wider than 0.0°.");
            }
            else if (verticalFieldOfView > 360.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalFieldOfView), "The field of view may not be wider than 360°.");
            }
            else
            {
                this.verticalFieldOfViewDegree = verticalFieldOfView;
            }

            if (viewDirection.Length() == 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(upDirection), "The upDirection has to be a direction. A vector of length 0.0 does not have a direction.");
            }
            else
            {
                this.viewDirection = viewDirection;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the projection matrix used by this camera.
        /// </summary>
        /// <remarks>
        /// Used to move an object from camera/view space to projection/screen space.
        /// </remarks>
        public Matrix ProjectionMatrix
        {
            get
            {
                // Lazy update of the projection matrix.
                if (this.projectionMatrixDirty)
                {
                    this.UpdateProjectionMatrix();
                }

                return this.projectionMatrix;
            }
        }

        /// <summary>
        /// Gets the view frustrum of this camera in world space.
        /// </summary>
        /// <remarks>
        /// Can be used, for example, to determine whether objects are within the camera's field of view.
        /// </remarks>
        public BoundingFrustum ViewFrustrum
        {
            get
            {
                // Lazy update of the view frustrum.
                if (this.viewFrustrumDirty)
                {
                    this.UpdateViewFrustrum();
                }

                return this.viewFrustrum;
            }
        }

        /// <summary>
        /// Gets the view matrix used by this camera.
        /// </summary>
        /// <remarks>
        /// Used to move an object from world to camera/vuew space.
        /// </remarks>
        public Matrix ViewMatrix
        {
            get
            {
                // Lazy update of the view matrix.
                if (this.viewMatrixDirty)
                {
                    this.UpdateViewMatrix();
                }

                return this.viewMatrix;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates the projection matrix used by this camera.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            // Monogame expects the vertical FOV to be specified as half aperture angle in radians.
            float verticalFovMonogame = MathHelper.ToRadians(this.verticalFieldOfViewDegree / 2.0f);

            Matrix newProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(verticalFovMonogame, this.aspectRatio, this.nearClippingPlaneDistance, this.farClippingPlaneDistance);
            this.projectionMatrixDirty = false;
            this.projectionMatrix = newProjectionMatrix;
        }

        /// <summary>
        /// Updates the view frustrum of this camera.
        /// </summary>
        private void UpdateViewFrustrum()
        {
            this.viewFrustrumDirty = false;
            this.viewFrustrum = new BoundingFrustum(this.ViewMatrix * this.ProjectionMatrix);
        }

        private void UpdateViewMatrix()
        {
            Vector3 lookAtTarget = this.position + this.viewDirection;
            Matrix newViewMatrix = Matrix.CreateLookAt(this.position, lookAtTarget, this.upDirection);

            this.viewMatrixDirty = false;
            this.viewMatrix = newViewMatrix;
        }
        #endregion
    }
}