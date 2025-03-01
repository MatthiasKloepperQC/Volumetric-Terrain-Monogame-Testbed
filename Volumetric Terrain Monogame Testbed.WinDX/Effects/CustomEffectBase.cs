﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SolConsulting.MonoGame.Effects
{
    enum EffectLanguage
    {
        Unknown = -1,
        HLSL = 0,
        GLSL = 2
    }

    internal class CustomEffectBase : Effect
    {
        #region Members
        #endregion

        #region Constructor
        internal CustomEffectBase(GraphicsDevice graphicsDevice, Type effectType) : base(graphicsDevice, CustomEffectBase.GetEffectByteCode(graphicsDevice, effectType))
        {
        }

        static CustomEffectBase()
        {
            CustomEffectBase.SystemEffectLanguage = EffectLanguage.Unknown;
        }
        #endregion

        #region Properties
        private static EffectLanguage SystemEffectLanguage { get; set; }
        #endregion

        #region Methods
        private static void DetectSupportedEffectLanguage(GraphicsDevice graphicsDevice, Type effectType)
        {
            CustomEffectBase.SystemEffectLanguage = EffectLanguage.Unknown;

            foreach (EffectLanguage language in Enum.GetValues(typeof(EffectLanguage)))
            {
                if (language == EffectLanguage.Unknown)
                {
                    continue;
                }

                byte[] effectByteCode;
                Effect temporaryEffect;
                try
                {
                    effectByteCode = CustomEffectBase.GetEmbeddedByteCode(effectType, language);
                    temporaryEffect = new Effect(graphicsDevice, effectByteCode);
                    if (temporaryEffect == null)
                    {
                        continue;
                    }
                    else
                    {
                        temporaryEffect.Dispose();
                        temporaryEffect = null;
                        CustomEffectBase.SystemEffectLanguage = language;
                        break;
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }

        private static byte[] GetEffectByteCode(GraphicsDevice graphicsDevice, Type effectType)
        {
            if (CustomEffectBase.SystemEffectLanguage == EffectLanguage.Unknown)
            {
                CustomEffectBase.DetectSupportedEffectLanguage(graphicsDevice, effectType);
            }

            if (CustomEffectBase.SystemEffectLanguage == EffectLanguage.Unknown)
            {
                throw new InvalidOperationException("Could not find any bytecode in a system compatible shader language for effect \"" + effectType.Name + "\"");
            }

            return GetEmbeddedByteCode(effectType, CustomEffectBase.SystemEffectLanguage);
        }


        private static byte[] GetEmbeddedByteCode(Type effectType, EffectLanguage language)
        {
            Assembly effectAssembly = effectType.Assembly;
            string resourceNamespace = effectType.Assembly.GetName().Name + ".Effects.ByteCode";
            string effectName = effectType.Name;
            string genericEffectByteCodeExtension = ".mgfxo";
            string effectLanguageExtension = language switch
            {
                EffectLanguage.GLSL => ".ogl",
                EffectLanguage.HLSL => ".dx11",
                _ => throw new ArgumentOutOfRangeException(nameof(language), "ByteCode can only be loaded for known shader languages (currently GLSL and HLSL)."),
            };
            string languageSpecificResourceName = effectName + effectLanguageExtension + genericEffectByteCodeExtension;

            string[] resourceNames = effectAssembly.GetManifestResourceNames();
            string fullResourceName = resourceNames.Where(resourceName => resourceName.EndsWith(languageSpecificResourceName)).FirstOrDefault();

            if (string.IsNullOrEmpty(fullResourceName))
            {
                throw new ArgumentException("Could not find embedded resource \"" + languageSpecificResourceName + "\". Existing embedded resources are: \"" +
                    string.Join("\", \"", resourceNames + "\"."));
            }

            byte[] byteCode;
            using (MemoryStream resourceMemStream = new MemoryStream())
            {
                using (Stream resourceStream = effectAssembly.GetManifestResourceStream(fullResourceName))
                {
                    resourceStream.CopyTo(resourceMemStream);
                }
                byteCode = resourceMemStream.ToArray();
            }

            return byteCode;
        }
        #endregion
    }
}