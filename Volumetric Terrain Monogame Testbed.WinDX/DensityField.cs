using System;
using System.Collections.Generic;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// A three dimensional density field.
    /// </summary>
    class DensityField
    {
        #region Members
        private (int x, int y, int z) dimension;
        #endregion

        #region Constructor
        private DensityField()
        {
            this.dimension = (256, 256, 256);
        }
        #endregion
    }
}