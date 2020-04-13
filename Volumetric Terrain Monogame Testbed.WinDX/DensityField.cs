using Microsoft.Xna.Framework;
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

            this.Dimension = (dimension, dimension, dimension);
            this.ScalarValues = new float[this.Dimension.X * this.Dimension.Y * this.Dimension.Z];
        }
        #endregion

        #region Properties
        /// <summary>
        /// The number of discrete sample points for each axis of the <see cref="DensityField"/>.
        /// </summary>
        internal (int X, int Y, int Z) Dimension { get; private set; }

        internal float[] ScalarValues { get; set; }
        #endregion

        #region Methods
        internal static DensityField FromSolidSphere(int dimension)
        {
            DensityField densityField = new DensityField(dimension);

            // Calculate the maximum radius of a sphere to fit into the density field.
            float radius = (dimension - 1) / 2.0f;
            Vector3 sphereCenter = new Vector3(radius);

            // DirectX textures use (0, 0, 0) as the (left, top, front)-point of a volumetric texture and (1, 1, 1) as the (right, bottom, back)-point.
            // The data from a one-dimensional array will be fit to the texture in u/v/w order.
            //
            // w := 0           w := 1
            // v\u |  0  1  2   v\u |  0  1  2
            // ----+---------   ----+---------
            //   0 |  0  1  2     0 |  9 10 11
            //   1 |  3  4  5     1 | 12 13 14
            //   2 |  6  7  8     2 | 15 16 17
            for (int w = 0; w < densityField.Dimension.Z; w++)
            {
                for (int v = 0; v < densityField.Dimension.Y; v++)
                {
                    for (int u = 0; u < densityField.Dimension.X; u++)
                    {
                        int scalarValuesIndex = u + (v * densityField.Dimension.X) + (w * densityField.Dimension.X * densityField.Dimension.Y);

                        densityField.ScalarValues[scalarValuesIndex] = ((new Vector3(u, v, w) - sphereCenter).Length() - radius) > 0 ? 0 : 1;
                    }
                }
            }

            return densityField;
        }
        #endregion
    }
}