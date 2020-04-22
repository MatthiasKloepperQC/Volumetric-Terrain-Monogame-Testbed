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
        WorldMatrix = 1,
        WorldViewProjectionMatrix = 2
    }

    class VolumeRaycastingEffect : CustomEffectBase
    {
        #region Members
        private EffectDirtyFlags dirtyFlags;
        private Matrix projectionMatrix = Matrix.Identity;
        private bool useVertexColor;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;
        private Matrix worldViewProjectionMatrix = Matrix.Identity;
        #endregion

        #region Constructor
        internal VolumeRaycastingEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice, typeof(VolumeRaycastingEffect))
        {
            this.Initialize();
        }
        #endregion

        #region Properties
        public Matrix ProjectionMatrix
        {
            get
            {
                return this.projectionMatrix;
            }
            set
            {
                this.projectionMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }

        public bool UseVertexColors
        {
            get
            {
                return this.useVertexColor;
            }
            set
            {
                this.useVertexColor = value;
                this.UpdateTechnique();
            }
        }

        public Matrix ViewMatrix
        {
            get
            {
                return this.viewMatrix;
            }
            set
            {
                this.viewMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }

        public Matrix WorldMatrix
        {
            get
            {
                return this.worldMatrix;
            }
            set
            {
                this.worldMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;
            }
        }

        public Matrix WorldViewProjectionMatrix
        {
            get
            {
                return this.worldViewProjectionMatrix;
            }
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            this.useVertexColor = false;
        }

        protected override void OnApply()
        {
            this.UpdateWorldViewProjectionMatrix();
            base.OnApply();
        }

        private void UpdateTechnique()
        {
            if (this.UseVertexColors)
            {
                this.CurrentTechnique = this.Techniques["VertexColors"];
            }
            else throw new InvalidOperationException("Keine Shader-Technik für die gesetzten Parameter vorhanden.");
        }

        private void UpdateWorldViewProjectionMatrix()
        {
            if ((this.dirtyFlags & EffectDirtyFlags.WorldViewProjectionMatrix) != 0)
            {
                // Reihenfolge: World, View, Projection.
                Matrix worldViewMatrix;
                Matrix.Multiply(ref this.worldMatrix, ref this.viewMatrix, out worldViewMatrix);
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