using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolConsulting.MonoGame.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattletechUniverse.Effects
{
    [Flags]
    enum EffectDirtyFlags
    {
        None = 0,

        /// <summary>
        /// Indicates that the WorldViewProjection matrix has to be recalculated.
        /// </summary>
        WorldViewProjectionMatrix = 1,

        /// <summary>
        /// Indicates that the inverse ViewProjection matrix has to be recalculated.
        /// </summary>
        InverseViewProjectionMatrix = 2
    }

    enum EffectMode
    {
        ColorFromScreenSpacePosition,
        ColorFromCameraRayDirection,
        FullVolumeRaycasting
    }

    /// <summary>
    /// Combines an index with a vertex buffer.
    /// </summary>
    struct CombinedBuffers
    {
        /// <summary>
        /// The index buffer.
        /// </summary>
        internal IndexBuffer Indices;

        /// <summary>
        /// The vertex buffer.
        /// </summary>
        internal VertexBuffer Vertices;
    }

    /// <summary>
    /// A monogame effect for volume raycasting.
    /// </summary>
    internal class VolumeRaycastingEffect : CustomEffectBase
    {
        #region Members
        private EffectDirtyFlags dirtyFlags;
        private CombinedBuffers fullScreenQuad;
        private Matrix inverseViewProjectionMatrix = Matrix.Identity;
        private EffectMode mode;
        private Matrix projectionMatrix = Matrix.Identity;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;
        private Matrix worldViewProjectionMatrix = Matrix.Identity;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new <see cref="VolumeRaycastingEffect"/> instance.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> instance to render this effect.</param>
        internal VolumeRaycastingEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice, typeof(VolumeRaycastingEffect))
        {
            this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            this.mode = EffectMode.ColorFromScreenSpacePosition;

            this.InitializeFullScreenQuad();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the position of the camera in world space.
        /// </summary>
        internal Vector3 CameraPositionWorldSpace { get; set; }

        internal EffectMode Mode
        {
            get => this.mode;
            set
            {
                this.mode = value;
                this.UpdateTechnique();
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix (view space -> screen space) for this <see cref="VolumeRaycastingEffect"/> instance.
        /// </summary>
        internal Matrix ProjectionMatrix
        {
            get => this.projectionMatrix;
            set
            {
                if (this.projectionMatrix != value)
                {
                    this.projectionMatrix = value;
                    this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix | EffectDirtyFlags.InverseViewProjectionMatrix;
                }
            }
        }

        /// <summary>
        /// Gets or sets the view matrix (world space -> view space) for this <see cref="VolumeRaycastingEffect"/> instance.
        /// </summary>
        internal Matrix ViewMatrix
        {
            get => this.viewMatrix;
            set
            {
                if (this.viewMatrix != value)
                {
                    this.viewMatrix = value;
                    this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix | EffectDirtyFlags.InverseViewProjectionMatrix;
                }
            }
        }

        /// <summary>
        /// Gets the inverse ViewProjection matrix (screen space -> world space) for this <see cref="VolumeRaycastingEffect"/> instance.
        /// </summary>
        internal Matrix InverseViewProjectionMatrix
        {
            get
            {
                if ((this.dirtyFlags & EffectDirtyFlags.InverseViewProjectionMatrix) != 0)
                {
                    this.UpdateInverseViewProjectionMatrix();
                }
                return this.inverseViewProjectionMatrix;
            }
        }

        /// <summary>
        /// Gets or sets the world matrix (object space -> world space) for this <see cref="VolumeRaycastingEffect"/> instance.
        /// </summary>
        internal Matrix WorldMatrix
        {
            get => this.worldMatrix;
            set
            {
                if (this.worldMatrix != value)
                {
                    this.worldMatrix = value;
                    this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
                }
            }
        }

        /// <summary>
        /// Gets the WorldViewProjection matrix (object space -> screen space) for this <see cref="VolumeRaycastingEffect"/> instance.
        /// </summary>
        internal Matrix WorldViewProjectionMatrix
        {
            get
            {
                if ((this.dirtyFlags & EffectDirtyFlags.WorldViewProjectionMatrix) != 0)
                {
                    this.UpdateWorldViewProjectionMatrix();
                }

                return this.worldViewProjectionMatrix;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws the a full screen quad to initialize the volume raycasting for all screen pixels.
        /// The <see cref="VolumeRaycastingEffect"/> needs to be set up accordingly before calling this function.
        /// </summary>
        internal void DrawFullScreenQuad()
        {
            this.GraphicsDevice.Indices = this.fullScreenQuad.Indices;
            this.GraphicsDevice.SetVertexBuffer(this.fullScreenQuad.Vertices);
            this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        /// <summary>
        /// Creates the vertices for rendering the full screen quad and set up the corresponding index and vertex buffers.
        /// </summary>
        private void InitializeFullScreenQuad()
        {
            if (this.fullScreenQuad.Indices == null)
            {
                // Create new index buffer for fullscreen quad.
                this.fullScreenQuad.Indices = new IndexBuffer(this.GraphicsDevice, typeof(int), 6, BufferUsage.WriteOnly);
            }

            if (this.fullScreenQuad.Vertices == null)
            {
                // Create new vertex buffer for fullscreen quad.
                this.fullScreenQuad.Vertices = new VertexBuffer(this.GraphicsDevice, typeof(VertexPosition), 4, BufferUsage.WriteOnly);
            }

            // Create the four vertices of the full screen quad in screen space with normalized device coordinates.
            VertexPosition[] vertexArray = new VertexPosition[4];
            // Bottom left.
            vertexArray[0] = new VertexPosition(new Vector3(-1.0f, -1.0f, 0.0f));
            // Bottom right.
            vertexArray[1] = new VertexPosition(new Vector3(1.0f, -1.0f, 0.0f));
            // Top left.
            vertexArray[2] = new VertexPosition(new Vector3(-1.0f, 1.0f, 0.0f));
            // Top right.
            vertexArray[3] = new VertexPosition(new Vector3(1.0f, 1.0f, 0.0f));

            // Fill vertex buffer with created vertices.
            this.fullScreenQuad.Vertices.SetData(vertexArray);

            // Create the six indices into the vertex buffer needed to render the fullscreen quad.
            int[] indexArray = new int[6];
            indexArray[0] = 0;
            indexArray[1] = 2;
            indexArray[2] = 3;
            indexArray[3] = 3;
            indexArray[4] = 1;
            indexArray[5] = 0;

            // Fill index buffer with created indices.
            this.fullScreenQuad.Indices.SetData(indexArray);
        }

        protected override void OnApply()
        {
            // Transfer uniform values to graphics device.
            this.Parameters["CameraPositionWorldSpace"].SetValue(this.CameraPositionWorldSpace);
            this.Parameters["InverseViewProjectionMatrix"].SetValue(this.InverseViewProjectionMatrix);
            //this.Parameters["WorldViewProjectionMatrix"].SetValue(this.WorldViewProjectionMatrix);
            base.OnApply();
        }

        /// <summary>
        /// Selects the appropriate technique of the shader base on the <see cref="Mode"/> property.
        /// </summary>
        private void UpdateTechnique()
        {
            this.CurrentTechnique = this.mode switch
            {
                EffectMode.ColorFromCameraRayDirection => this.Techniques["ColorFromCameraRayDirection"],
                EffectMode.ColorFromScreenSpacePosition => this.Techniques["ColorFromScreenSpacePosition"],
                EffectMode.FullVolumeRaycasting => this.Techniques["FullVolumeRaycasting"],
                _ => throw new InvalidOperationException("No existing shader technique for this mode."),
            };
        }

        /// <summary>
        /// Updates the inverse ViewProjection matrix.
        /// </summary>
        private void UpdateInverseViewProjectionMatrix()
        {
            if ((this.dirtyFlags & EffectDirtyFlags.InverseViewProjectionMatrix) != 0)
            {
                // Calculate the ViewProjection matrix.
                // Order: View, Projection.
                Matrix.Multiply(ref this.viewMatrix, ref this.projectionMatrix, out Matrix viewProjectionMatrix);

                // Calculate the inverse of the ViewProjection matrix.
                Matrix.Invert(ref viewProjectionMatrix, out this.inverseViewProjectionMatrix);

                // The inverse ViewProjection matrix is up tp date now.
                this.dirtyFlags ^= EffectDirtyFlags.InverseViewProjectionMatrix;
            }
        }

        /// <summary>
        /// Updates the WorldViewProjection matrix.
        /// </summary>
        private void UpdateWorldViewProjectionMatrix()
        {
            if ((this.dirtyFlags & EffectDirtyFlags.WorldViewProjectionMatrix) != 0)
            {
                // Calculate the WorldViewProjection matrix.
                // Order: World, View, Projection.
                Matrix.Multiply(ref this.worldMatrix, ref this.viewMatrix, out Matrix worldViewMatrix);
                Matrix.Multiply(ref worldViewMatrix, ref this.projectionMatrix, out this.worldViewProjectionMatrix);

                // The WorldViewProjection matrix is up to date now.
                this.dirtyFlags ^= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }
        #endregion
    }
}