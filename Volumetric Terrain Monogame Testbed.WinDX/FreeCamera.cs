using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Represents a bit field that indicates which properties have to be updated before their next use.
    /// </summary>
    [Flags]
    enum CameraDirtyFlags : uint
    {
        /// <summary>
        /// Indicates that all properties are up to date.
        /// </summary>
        None = 0b00000000_00000000_00000000_00000000,

        /// <summary>
        /// Indicates that the UpdateId has to be incremented.
        /// </summary>
        UpdateId = 0b00000000_00000000_00000000_00000001,

        /// <summary>
        /// Indicates that the projection matrix has to be recalculated.
        /// </summary>
        ProjectionMatrix = 0b00000000_00000000_00000000_00000010,

        /// <summary>
        /// Indicates that the view matrix has to be recalculated.
        /// </summary>
        ViewMatrix = 0b00000000_00000000_00000000_00000100,

        /// <summary>
        /// Indicates that the view frustrum has to be recalculated.
        /// </summary>
        ViewFrustrum = 0b00000000_00000000_00000000_00001000
    }

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
        /// The bit field that indicates which properties have to be updated on their next use.
        /// </summary>
        private CameraDirtyFlags dirtyFlags;

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
        /// The direction in world space that is considered "right" for the camera's current rotation.
        /// </summary>
        private Vector3 rightDirection;

        /// <summary>
        /// A value that is incremented with every state changing update.
        /// </summary>
        private uint updateId;

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
        /// The view matrix of this camera.
        /// Used to move an object from world space to camera/view space.
        /// </summary>
        private Matrix viewMatrix;

        /// <summary>
        /// Describes the current rotation of this camera.
        /// </summary>
        private Quaternion rotation;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new FreeCamera instance using some default values.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> from which to deduct some settings for the new FreeCamera instance.</param>
        /// <remarks>
        /// Position will be set to the world space origin (0.0, 0.0, 0.0).
        /// Viewing direction will be set to (0.0, 0.0, -1.0) (= "into the screen").
        /// Up direction will be set to (0.0, 1.0, 0.0) (= "up as normally perceived").
        /// The vertical field of view will be set to 90°.
        /// The aspect ratio will be set to the aspect ratio of the passed GraphicsDevice's viewport.
        /// The near clipping plane will be placed at a distance of 1.0.
        /// The far clipping plane will be placed at a distance of 10,000.0.
        /// </remarks>
        internal FreeCamera(GraphicsDevice device) : this(Vector3.Zero, FreeCamera.defaultViewingDirection, FreeCamera.defaultUpDirection, 90.0f, 
            device.Viewport.AspectRatio, 1.0f, 10000.0f)
        {
        }

        /// <summary>
        /// Creates a new FreeCamera instance.
        /// </summary>
        /// <param name="position">The starting position of the camera in world space.</param>
        /// <param name="viewDirection">The initial viewing direction (not the target point!) of the camera in world space.</param>
        /// <param name="upDirection">The initial up direction of the camera.</param>
        /// <param name="verticalFieldOfView">The vertical field of view / the vertical aperture angle (https://en.wikipedia.org/wiki/Field_of_view_in_video_games) of the camera in degrees.</param>
        /// <param name="aspectRatio">The aspect ratio of the camera.</param>
        /// <param name="nearClippingPlaneDistance">The distance to the near clipping plane.</param>
        /// <param name="farClippingPlaneDistance">The distance to the far clipping plane.</param>
        internal FreeCamera(Vector3 position, Vector3 viewDirection, Vector3 upDirection, float verticalFieldOfView, float aspectRatio, 
            float nearClippingPlaneDistance, float farClippingPlaneDistance)
        {
            // Default values.
            this.rotation = Quaternion.Identity;
            this.projectionMatrix = Matrix.Identity;
            this.viewMatrix = Matrix.Identity;
            this.dirtyFlags = CameraDirtyFlags.UpdateId | CameraDirtyFlags.ProjectionMatrix | CameraDirtyFlags.ViewMatrix | CameraDirtyFlags.ViewFrustrum;

            // Check and transfer values from call.
            if (aspectRatio < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio), "The aspect ration has to be greater than zero.");
            }
            else
            {
                this.aspectRatio = aspectRatio;
            }

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
                if ((this.dirtyFlags & CameraDirtyFlags.ProjectionMatrix) != 0)
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
                if ((this.dirtyFlags & CameraDirtyFlags.ViewFrustrum) != 0)
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
        /// Used to move an object from world to camera/view space.
        /// </remarks>
        public Matrix ViewMatrix
        {
            get
            {
                // Lazy update of the view matrix.
                if ((this.dirtyFlags & CameraDirtyFlags.ViewMatrix) != 0)
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

            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(verticalFovMonogame, this.aspectRatio, this.nearClippingPlaneDistance, this.farClippingPlaneDistance);
            this.dirtyFlags ^= CameraDirtyFlags.ProjectionMatrix;
            this.dirtyFlags |= CameraDirtyFlags.UpdateId;
        }

        /// <summary>
        /// Updates the view frustrum of this camera.
        /// </summary>
        private void UpdateViewFrustrum()
        {
            this.viewFrustrum = new BoundingFrustum(this.ViewMatrix * this.ProjectionMatrix);
            this.dirtyFlags ^= CameraDirtyFlags.ViewFrustrum;
            this.dirtyFlags |= CameraDirtyFlags.UpdateId;
        }

        private void UpdateViewMatrix()
        {
            Vector3 lookAtTarget = this.position + this.viewDirection;
            Matrix newViewMatrix = Matrix.CreateLookAt(this.position, lookAtTarget, this.upDirection);

            this.viewMatrix = newViewMatrix;
            this.dirtyFlags ^= CameraDirtyFlags.ViewMatrix;
            this.dirtyFlags |= CameraDirtyFlags.UpdateId;
        }
        #endregion
    }
}