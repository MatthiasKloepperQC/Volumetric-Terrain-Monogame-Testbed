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
    /// A DebugInfo component that collects information about the graphics system's environment.
    /// </summary>
    internal class DebugInfoGraphicsEnvironment : IDebugInfoComponent, IUpdateableComponent
    {
        #region Fields
        /// <summary>
        /// The <see cref="graphicsDevice"/> whose environment is to be checked.
        /// </summary>
        private readonly GraphicsDevice graphicsDevice;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="DebugInfoGraphicsEnvironment"/> instance with all "order"-Properties set to zero.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device from which environment data is to be collected.</param>
        internal DebugInfoGraphicsEnvironment(GraphicsDevice graphicsDevice) : this(graphicsDevice, 0, 0) { }

        /// <summary>
        /// Creates a new <see cref="DebugInfoGraphicsEnvironment"/> instance with all "order"-Properties set to zero.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device from which environment data is to be collected.</param>
        /// <param name="debugOrder">Defines at which point in the draw sequence this component is drawn.</param>
        /// <param name="updateOrder">Defines at which point in the update sequence this component is updated.</param>
        internal DebugInfoGraphicsEnvironment(GraphicsDevice graphicsDevice, int debugOrder, int updateOrder)
        {
            // Check parameters and set fields.
            this.Description = "Graphics Environment";
            this.DebugInfo = string.Empty;
            this.DebugOrder = debugOrder;
            this.DebugOutputEnabled = true;
            this.Enabled = true;
            this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice), "The graphics device must be given.");
            this.UpdateOrder = updateOrder;

            // TODO: Hook into events of GraphicsDevice to detect changed settings instead of polling in each Update.
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the debugging info text from this instance.
        /// </summary>
        public string DebugInfo { get; internal set; }

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

        /// <summary>
        /// Gets or sets a value that defines at which point in the update sequence this component is updated.
        /// </summary>
        public int UpdateOrder { get; internal set; }
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

        /// <summary>
        /// Compares this instance to the specified <see cref="IUpdateableComponent"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="IUpdateableComponent"/> instance to compare to this instance.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>A negative number if this instance shall be updated before the <paramref name="other"/> instance.</item>
        ///         <item>Zero if the this instance and the <paramref name="other"/> instance may be updated in any order.</item>
        ///         <item>A positive number if this instance shall be updated after the <paramref name="other"/> instance.</item>
        ///     </list>
        /// </returns>
        public int CompareTo(IUpdateableComponent other)
        {
            return other == null ? -1 : this == other ? 0 : this.UpdateOrder.CompareTo(other.UpdateOrder);
        }

        /// <summary>
        /// Called when the game determines to update the state of this instance.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Update(GameTime)"/>.</param>
        public void Update(GameTime gameTime)
        {
            this.DebugInfo = "Device: " + this.graphicsDevice.Adapter.Description +
                "\nResolution: " + this.graphicsDevice.PresentationParameters.BackBufferWidth + " x " + this.graphicsDevice.PresentationParameters.BackBufferHeight +
                "\nFull screen: " + this.graphicsDevice.PresentationParameters.IsFullScreen;
        }
        #endregion
    }
}