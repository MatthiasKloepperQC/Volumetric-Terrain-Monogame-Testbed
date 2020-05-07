using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Defines an interface for a game component that needs to be updated regularly by the game logic.
    /// </summary>
    internal interface IUpdateableComponent : IComparable<IUpdateableComponent>, IGameComponent
    {
        #region Properties
        /// Gets a value that defines at which point in the update sequence this component is updated.
        public int UpdateOrder { get; }
        #endregion

        #region Methods
        /// <summary>>
        /// Called when the game determines to update the state of this instance.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Update(GameTime)"/>.</param>
        public void Update(GameTime gameTime);
        #endregion
    }
}