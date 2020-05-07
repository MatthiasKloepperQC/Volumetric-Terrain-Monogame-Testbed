using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Defines an interface for a game component that can be drawn.
    /// </summary>
    internal interface IDrawableComponent : IComparable<IDrawableComponent>, IGameComponent
    {
        #region Properties
        /// <summary>
        /// Gets a value that defines at which point in the draw sequence this component is drawn.
        /// </summary>
        public int DrawOrder { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Called when the game determines it is time to draw a frame with this component.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Draw(GameTime)"/>.</param>
        public void Draw(GameTime gameTime);
        #endregion
    }
}