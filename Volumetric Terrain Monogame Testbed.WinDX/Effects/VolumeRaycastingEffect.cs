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
        /// Indicates that the world matrix has to be recalculated.
        /// </summary>
        WorldMatrix = 1,

        /// <summary>
        /// Indicates that the world-view-projection matrix has to be recalculated.
        /// </summary>
        WorldViewProjectionMatrix = 2
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
    class VolumeRaycastingEffect : CustomEffectBase
    {
        #region Members
        private EffectDirtyFlags dirtyFlags;
        private CombinedBuffers fullScreenQuad;
        private Matrix projectionMatrix = Matrix.Identity;
        private bool useRaymarchFullScreen;
        private bool useVertexColor;
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
            this.dirtyFlags = EffectDirtyFlags.WorldMatrix | EffectDirtyFlags.WorldViewProjectionMatrix;
            this.useRaymarchFullScreen = true;
            this.useVertexColor = false;

            this.InitializeFullScreenQuad();
        }
        #endregion

        #region Properties
        public CombinedBuffers FullScreenQuad => this.fullScreenQuad;

        public Matrix ProjectionMatrix
        {
            get => this.projectionMatrix;
            set
            {
                this.projectionMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }

        public bool UseRaymarchFullScreen
        {
            get => this.useRaymarchFullScreen;
            set
            {
                this.useRaymarchFullScreen = value;
                this.useVertexColor = false;
                this.UpdateTechnique();
            }
        }

        public bool UseVertexColor
        {
            get => this.useVertexColor;
            set
            {
                this.useVertexColor = value;
                this.useRaymarchFullScreen = false;
                this.UpdateTechnique();
            }
        }

        public Matrix ViewMatrix
        {
            get => this.viewMatrix;
            set
            {
                this.viewMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }

        public Matrix WorldMatrix
        {
            get => this.worldMatrix;
            set
            {
                this.worldMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }

        public Matrix WorldViewProjectionMatrix => this.worldViewProjectionMatrix;
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
            this.UpdateWorldViewProjectionMatrix();
            base.OnApply();
        }

        private void UpdateTechnique()
        {
            if (this.UseRaymarchFullScreen)
            {
                this.CurrentTechnique = this.Techniques["RaymarchFullScreen"];
            }
            else if (this.UseVertexColor)
            {
                this.CurrentTechnique = this.Techniques["VertexColors"];
            }
            else
            {
                throw new InvalidOperationException("Keine Shader-Technik für die gesetzten Parameter vorhanden.");
            }
        }

        private void UpdateWorldViewProjectionMatrix()
        {
            if ((this.dirtyFlags & EffectDirtyFlags.WorldViewProjectionMatrix) != 0)
            {
                // Reihenfolge: World, View, Projection.
                Matrix.Multiply(ref this.worldMatrix, ref this.viewMatrix, out Matrix worldViewMatrix);
                Matrix.Multiply(ref worldViewMatrix, ref this.projectionMatrix, out this.worldViewProjectionMatrix);

                // Die WorldViewProjection-Matrix ist jetzt frisch berechnet.
                this.dirtyFlags ^= EffectDirtyFlags.WorldViewProjectionMatrix;

                // An die Grafikkarte übergeben.
                this.Parameters["WorldViewProjectionMatrix"].SetValue(this.worldViewProjectionMatrix);
            }
        }
        #endregion
    }
}