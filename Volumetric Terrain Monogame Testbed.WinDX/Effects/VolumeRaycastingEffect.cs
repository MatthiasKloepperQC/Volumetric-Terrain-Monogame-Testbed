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
    {/*
        #region Members
        private EffectDirtyFlags dirtyFlags;
        private Matrix modelRotationMatrix = Matrix.Identity;
        private Matrix modelScaleMatrix = Matrix.Identity;
        private Matrix modelTranslationMatrix = Matrix.Identity;
        private Matrix projectionMatrix = Matrix.Identity;
        private bool useSingleTexture;
        private bool useVertexColor;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;
        private Matrix worldViewProjectionMatrix = Matrix.Identity;
        #endregion*/

        #region Constructor
        internal VolumeRaycastingEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice, typeof(VolumeRaycastingEffect))
        {
            /*
            this.Initialize();
            */
        }
        #endregion
        /*
        #region
        public Matrix ModelRotationMatrix
        {
            get
            {
                return this.modelRotationMatrix;
            }
            set
            {
                this.modelRotationMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldMatrix;
            }
        }

        public Matrix ModelScaleMatrix
        {
            get
            {
                return this.modelScaleMatrix;
            }
            set
            {
                this.modelScaleMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldMatrix;
            }
        }

        public Matrix ModelTranslationMatrix
        {
            get
            {
                return this.modelTranslationMatrix;
            }
            set
            {
                this.modelTranslationMatrix = value;
                this.dirtyFlags |= EffectDirtyFlags.WorldMatrix;
            }
        }

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

        public bool UseSingleTexture
        {
            get
            {
                return this.useSingleTexture;
            }
            set
            {
                this.useSingleTexture = value;
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
                this.UpdateWorldMatrix();
                return this.worldMatrix;
            }
        }

        public Matrix WorldViewProjectionMatrix
        {
            get
            {
                this.UpdateWorldViewProjectionMatrix();
                return this.worldViewProjectionMatrix;
            }
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            this.dirtyFlags = EffectDirtyFlags.None;

            this.ModelRotationMatrix = Matrix.Identity;
            this.ModelScaleMatrix = Matrix.Identity;
            this.ModelTranslationMatrix = Matrix.Identity;
            this.ProjectionMatrix = Matrix.Identity;
            this.ViewMatrix = Matrix.Identity;
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
            else if (this.useSingleTexture)
            {
                this.CurrentTechnique = this.Techniques["SingleTexture"];
            }
            //else throw new InvalidOperationException("Keine Shader-Technik für die gesetzten Parameter vorhanden.");
        }

        private void UpdateWorldMatrix()
        {
            if ((this.dirtyFlags & EffectDirtyFlags.WorldMatrix) != 0)
            {
                // Reihenfolge: Scale, Rotation, Translation.
                Matrix modelScaleRotationMatrix;
                Matrix.Multiply(ref this.modelScaleMatrix, ref this.modelRotationMatrix, out modelScaleRotationMatrix);
                Matrix.Multiply(ref modelScaleRotationMatrix, ref this.modelTranslationMatrix, out this.worldMatrix);

                // Die World-Matrix ist jetzt frisch berechnet.
                this.dirtyFlags ^= EffectDirtyFlags.WorldMatrix;

                // Dadurch muss aber vermutlich die WorldViewProjection-Matrix neu berechnet werden.
                this.dirtyFlags |= EffectDirtyFlags.WorldViewProjectionMatrix;

                // An die Grafikkarte übergeben.
                if (this.Parameters.Where(parameter => parameter.Name == "WorldMatrix").Count() > 0)
                {
                    this.Parameters["WorldMatrix"].SetValue(this.worldMatrix);
                }
            }
        }

        private void UpdateWorldViewProjectionMatrix()
        {
            this.UpdateWorldMatrix();

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
        */
    }
}