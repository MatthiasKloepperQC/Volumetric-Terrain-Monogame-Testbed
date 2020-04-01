using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain.Effects

    // Change this to auto detect working bytecode platform type on loading of first effect (load all available suffixes), remember the one that worked.
{
    internal class EffectHelper
    {/*
        #region Members
        private static Assembly assembly;
        private static Dictionary<GraphicsDevice, EffectHelper> deviceInstance = new Dictionary<GraphicsDevice, EffectHelper>(1);
        private GraphicsDevice graphicsDevice;
        private static Type type;
        private static TypeInfo typeInfo;
        #endregion

        #region Constructor
        static EffectHelper()
        {
            EffectHelper.type = typeof(EffectHelper);
            EffectHelper.typeInfo = EffectHelper.type.GetTypeInfo();
            EffectHelper.assembly = EffectHelper.typeInfo.Assembly;
        }

        private EffectHelper(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }
        #endregion

        #region Methods
        private byte[] GetByteCodeFromEmbeddedResource(string fullQualifiedResourceName)
        {
            byte[] byteCode;

            using (MemoryStream resourceMemStream = new MemoryStream())
            {
                using (Stream resourceStream = EffectHelper.assembly.GetManifestResourceStream(fullQualifiedResourceName))
                {
                    if (resourceStream == null)
                    {
                        throw new ArgumentException("Die Ressource \"" + fullQualifiedResourceName + "\" ist nicht vorhanden. Vorhandene Ressourcen sind: \"" + 
                            String.Join("\", \"", EffectHelper.assembly.GetManifestResourceNames()) + "\".");
                    }

                    resourceStream.CopyTo(resourceMemStream);
                }
                byteCode = resourceMemStream.ToArray();
            }

            return byteCode;
        }

        internal byte[] GetEffectByteCode(Type effectType)
        {
            return this.GetEffectByteCode(effectType, PlatformCharacteristics.GraphicsPlatform);
        }

        private byte[] GetEffectByteCode(Type effectType, GraphicsPlatform platform)
        {
            string resourceNamespace = EffectHelper.typeInfo.Namespace + ".Effects.ByteCode";
            string effectName = effectType.Name;
            string genericEffectByteCodeExtension = ".mgfxo";

            string platformSpecificExtension;
            switch (platform)
            {
                case GraphicsPlatform.DirectX:
                    platformSpecificExtension = ".dx11";
                    break;
                case GraphicsPlatform.OpenGL:
                    platformSpecificExtension = ".ogl";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("platform", "Für die Grafik-Plattform " + platform.ToString() + " ist keine Dateinamenerweiterung bekannt. Der Bytecode für diese " +
                        "Plattform kann daher nicht geladen werden.");
            }

            string platformSpecificResourceName = resourceNamespace + "." + effectName + platformSpecificExtension + genericEffectByteCodeExtension;

            try
            {
                byte[] effectByteCode = null;
                effectByteCode = this.GetByteCodeFromEmbeddedResource(platformSpecificResourceName);

                return effectByteCode;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Für den Effekt " + effectType.FullName + " konnte kein kompatibler Bytecode für die Grafik-Plattform " + platform.ToString() + " geladen werden.", e);
            }
        }

        internal static EffectHelper GetEffectHelper(GraphicsDevice graphicsDevice)
        {
            if (!EffectHelper.deviceInstance.ContainsKey(graphicsDevice)) EffectHelper.deviceInstance.Add(graphicsDevice, null);

            EffectHelper deviceEffectHelper = EffectHelper.deviceInstance[graphicsDevice];
            if (deviceEffectHelper == null)
            {
                deviceEffectHelper = new EffectHelper(graphicsDevice);
                EffectHelper.deviceInstance[graphicsDevice] = deviceEffectHelper;
            }

            return deviceEffectHelper;
        }

        private GraphicsPlatform DetectGraphicsPlatformByBytecodeTesting()
        {
            // Zur Feststellung der verwendeten Grafik-Plattform wird einer der bekannten Effekte in den Bytecode-Formaten für die verschiedenen Plattformen geladen.
            // Der erste erfolgreiche Ladevorgang zeigt dabei, welche Grafik-Platform verwendet wird.
            byte[] effectByteCode = null;
            Type effectType = typeof(BattletechEffect);
            Effect platformSpecificEffect = null;
            GraphicsPlatform graphicsPlatform = GraphicsPlatform.Unknown;

            // Alle bekannten Grafik-Plattformen durchgehen.
            foreach (GraphicsPlatform currentPlatform in Enum.GetValues(typeof(GraphicsPlatform)))
            {
                if (currentPlatform == GraphicsPlatform.Unknown) continue;

                try
                {
                    effectByteCode = this.GetEffectByteCode(effectType, currentPlatform);
                    platformSpecificEffect = new Effect(this.graphicsDevice, effectByteCode);

                    // Wenn der Effect mit diesem Bytecode ohne Auslösen einer Exception geladne wurde, dann ist die getestete
                    // Grafik-Plattform diejenige, die aktuell verwendet wird.
                    graphicsPlatform = currentPlatform;

                    // Dei anderen ByteCode-Varianten müssen dann auch nicht mehr getestet werden.
                    break;
                }
                catch { }
            }

            if (platformSpecificEffect == null)
            {
                IEnumerable<GraphicsPlatform> knownPlatforms = ((GraphicsPlatform[])Enum.GetValues(typeof(GraphicsPlatform))).Where(platform => platform != GraphicsPlatform.Unknown);
                string testedGraphicPlatforms = string.Join(", ", knownPlatforms.Select(platform => platform.ToString()));
                throw new Exception("Die verwendete Grafik-Plattform konnte nicht erkannt werden. (Test-Effekt: " + effectType.FullName + "; Gesteste Plattformen: " + testedGraphicPlatforms + ".");
            }
            else
            {
                // Den Effekt wieder entladen. War nur zum Testen der verwendeten Grafik-Plattform gedacht.
                if (platformSpecificEffect != null) platformSpecificEffect.Dispose();
                platformSpecificEffect = null;
            }

            return graphicsPlatform;
        }
        #endregion
        */
    }
}