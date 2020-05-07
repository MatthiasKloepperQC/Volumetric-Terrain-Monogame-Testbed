using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Defines an interface for a game component that provides debugging information.
    /// </summary>
    interface IDebugInfoComponent : IComparable<IDebugInfoComponent>, IGameComponent
    {
        #region Properties
        /// <summary>
        /// Gets the current debugging info text of this instance.
        /// </summary>
        public string DebugInfo { get; }

        /// <summary>
        /// Gets a value that defines at which point in the debug sequence this component's debugInfo is presented.
        /// </summary>
        public int DebugOrder { get; }

        /// <summary>
        /// Gets wether the debugging info from this instance should currently be used in any form of debugging output.
        /// </summary>
        public bool DebugOutputEnabled { get; }

        /// <summary>
        /// Gets the description / heading / category that should be shown to categorize the debugging info from this instance.
        /// </summary>
        public string Description { get; }
        #endregion
    }
}