using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// A DebugInfo component that collects information about the software environment.
    /// </summary>
    internal class DebugInfoSoftwareEnvironment : IDebugInfoComponent
    {
        #region Constructor
        /// <summary>
        /// Creates a new <see cref="DebugInfoSoftwareEnvironment"/> instance with the <see cref="DebugOrder"/> property set to zero.
        /// </summary>
        internal DebugInfoSoftwareEnvironment() : this(0) { }

        /// <summary>
        /// Creates a new <see cref="DebugInfoSoftwareEnvironment"/> instance.
        /// </summary>
        internal DebugInfoSoftwareEnvironment(int debugOrder)
        {
            this.Description = "Software Environment";
            this.DebugInfo = "Operating System: " + System.Environment.OSVersion.VersionString +
                  (System.Environment.Is64BitOperatingSystem ? " (64-bit)" : "") +
                  "\nMonogame: " + Assembly.GetAssembly(typeof(GraphicsDevice)).GetName().Version +
                  "\nProcess: " + Assembly.GetEntryAssembly().GetName().Name + " V" + Assembly.GetEntryAssembly().GetName().Version +
                  (System.Environment.Is64BitProcess ? " (64-bit)" : "");
            this.DebugOrder = debugOrder;
            this.DebugOutputEnabled = true;
            this.Enabled = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the debugging info text from this instance.
        /// </summary>
        public string DebugInfo { get; }

        /// <summary>
        /// Gets the order in which the debugging info should be presented.
        /// </summary>
        public int DebugOrder { get; internal set; }

        /// <summary>
        /// Gets wether the debugging info from this instance should currently be used in any form of debugging output.
        /// </summary>
        public bool DebugOutputEnabled { get; internal set; }

        /// <summary>
        /// Gets the description / heading / category that should be shown to categorize the debugging info from this instance.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets whether this instance should currently be used by other components.
        /// </summary>
        public bool Enabled { get; internal set; }
        #endregion

        #region Methods
        /// <summary>
        /// Compares this instance to the specified <see cref="IDebugInfoComponent"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="IDebugInfoComponent"/> instance to compare to this instance.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>A negative number if this instance's debugging info shall be presented before the <paramref name="other"/> instance's debugging info.</item>
        ///         <item>Zero if the debugging info of this instance and the <paramref name="other"/> instance may be presented in any order.</item>
        ///         <item>A positive number if this instance's debugging info shall be presented after the <paramref name="other"/> instance's debugging info.</item>
        ///     </list>
        /// </returns>
        public int CompareTo(IDebugInfoComponent other)
        {
            return other == null ? -1 : this == other ? 0 : this.DebugOrder.CompareTo(other.DebugOrder);
        }
        #endregion
    }
}