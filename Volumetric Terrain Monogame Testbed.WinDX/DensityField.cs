using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// A three dimensional density field with the discrete sample points layed out in a regular grid.
    /// </summary>
    internal class DensityField
    {
        #region Constants
        /// <summary>
        /// The default dimension for each axis of the <see cref="DensityField"/>.
        /// </summary>
        private const int DefaultDimension=256;
        #endregion

        #region Members
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="DensityField"/> instance using the <see cref="DefaultDimension"/> for each axis.
        /// </summary>
        private DensityField() : this(DensityField.DefaultDimension) { }

        private DensityField(int dimension)
        {
            // Check parameters.
            if (dimension < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), "Each dimension of the " + nameof(DensityField) + " has to be greater than zero.");
            }

            this.Dimension = (256, 256, 256);
            this.ScalarValues = new double[this.Dimension.X * this.Dimension.Y * this.Dimension.Z];
        }
        #endregion

        #region Properties
        /// <summary>
        /// The number of discrete sample points for each axis of the <see cref="DensityField"/>.
        /// </summary>
        internal (int X, int Y, int Z) Dimension { get; private set; }

        internal double[] ScalarValues { get; set; }
        #endregion
    }
}