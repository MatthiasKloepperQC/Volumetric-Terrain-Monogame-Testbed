using System;
using System.Collections.Generic;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Necessary interface for components managed through a <see cref="ComponentManager"/> instance.
    /// </summary>
    internal interface IGameComponent
    {
        #region Properties
        /// <summary>
        /// Gets whether this instance should currently be used by other components.
        /// </summary>
        public bool Enabled { get; }
        #endregion
    }
}