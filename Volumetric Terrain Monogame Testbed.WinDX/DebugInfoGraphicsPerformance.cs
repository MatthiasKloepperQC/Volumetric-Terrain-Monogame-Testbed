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
    /// A DebugInfo component that collects information about the graphics system's performance.
    /// </summary>
    internal class DebugInfoGraphicsPerformance : IDebugInfoComponent, IDrawableComponent, IUpdateableComponent
    {
        #region Fields
        /// <summary>
        /// <see cref="TimeSpan"/> that has to elapse betwenn two checks for performance data.
        /// </summary>
        private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Number of draw calls per frame.
        /// </summary>
        private long drawsPerFrame;

        /// <summary>
        /// The number of draw calls since the last check for performance data.
        /// </summary>
        private long drawsSinceLastCheck;

        /// <summary>
        /// <see cref="TimeSpan"/> elapsed since the last check for performance data.
        /// </summary>
        private TimeSpan elapsedSinceLastCheck;

        /// <summary>
        /// Number of frames drawn since the last check for performance data.
        /// </summary>
        private int framesSinceLastCheck;

        /// <summary>
        /// The <see cref="graphicsDevice"/> that should be checked for performance data.
        /// </summary>
        private readonly GraphicsDevice graphicsDevice;

        /// <summary>
        /// Number of primitives drawn per frame.
        /// </summary>
        private long primitivesPerFrame;

        /// <summary>
        /// Number of primitives drawn since the last check for performance data.
        /// </summary>
        private long primitivesSinceLastCheck;

        /// <summary>
        /// Number of calls to <see cref="Update(GameTime)"/> since the last check for performance data.
        /// </summary>
        private int updatesSinceLastCheck;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="DebugInfoGraphicsPerformance"/> instance with all "order"-Properties set to zero.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device from which performance data is to be collected.</param>
        internal DebugInfoGraphicsPerformance(GraphicsDevice graphicsDevice) : this(graphicsDevice, 0, 0, 0) { }

        /// <summary>
        /// Creates a new <see cref="DebugInfoGraphicsPerformance"/> instance.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device from which performance data is to be collected.</param>
        /// <param name="debugOrder">Defines at which point in the debug sequence this component's debugInfo is presented.</param>
        /// <param name="drawOrder">Defines at which point in the draw sequence this component is drawn.</param>
        /// <param name="updateOrder">Defines at which point in the update sequence this component is updated.</param>
        internal DebugInfoGraphicsPerformance(GraphicsDevice graphicsDevice, int debugOrder, int drawOrder, int updateOrder)
        {
            // Check parameters and set fields.
            this.DebugInfo = string.Empty;
            this.DebugOrder = debugOrder;
            this.DebugOutputEnabled = true;
            this.Description = "Graphics Performance";
            this.DrawOrder = drawOrder;
            this.Enabled = true;
            this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice), "The graphics device must be given.");
            this.UpdateOrder = updateOrder;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current debugging info text of this instance.
        /// </summary>
        public string DebugInfo { get; internal set; }

        /// <summary>
        /// Gets or sets a value that defines at which point in the debug sequence this component's debugInfo is presented.
        /// </summary>
        public int DebugOrder { get; internal set; }

        /// <summary>
        /// Gets or sets wether the debugging info from this instance should currently be used in any form of debugging output.
        /// </summary>
        public bool DebugOutputEnabled { get; internal set; }

        /// <summary>
        /// Gets the description / heading / category that should be shown to categorize the debugging info from this instance.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets a value that defines at which point in the draw sequence this component is drawn.
        /// </summary>
        public int DrawOrder { get; internal set; }

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
        /// Compares this instance to the specified <see cref="IDrawableComponent"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="IDrawableComponent"/> instance to compare to this instance.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>A negative number if this instance shall be drawn before the <paramref name="other"/> instance.</item>
        ///         <item>Zero if the this instance and the <paramref name="other"/> instance may be drawn in any order.</item>
        ///         <item>A positive number if this instance shall be drawn after the <paramref name="other"/> instance.</item>
        ///     </list>
        /// </returns>
        public int CompareTo(IDrawableComponent other)
        {
            return other == null ? -1 : this == other ? 0 : this.DrawOrder.CompareTo(other.DrawOrder);
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
        /// Called when the game determines it is time to draw a frame with this component.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Draw(GameTime)"/>.</param>
        public void Draw(GameTime gameTime)
        {
            // The graphics system wants to draw this instance.
            // Increase the frame count instead of a real draw.
            this.framesSinceLastCheck++;
            this.drawsPerFrame = this.graphicsDevice.Metrics.DrawCount;
            this.drawsSinceLastCheck += this.graphicsDevice.Metrics.DrawCount;
            this.primitivesPerFrame = this.graphicsDevice.Metrics.PrimitiveCount;
            this.primitivesSinceLastCheck += this.graphicsDevice.Metrics.PrimitiveCount;
        }

        /// <summary>
        /// Called when the game determines to update the state of this instance.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Update(GameTime)"/>.</param>
        public void Update(GameTime gameTime)
        {
            // Update time passed since the last check for performance data.
            this.elapsedSinceLastCheck += gameTime.ElapsedGameTime;
            this.updatesSinceLastCheck++;

            // Determine if the specified check interval has elapsed.
            if (this.elapsedSinceLastCheck > this.checkInterval)
            {
                // Update performance data based on the elapsed time since the last check for performance data.
                int drawsPerSecond = (int)Math.Round(this.drawsSinceLastCheck / this.elapsedSinceLastCheck.TotalSeconds, 0);
                int framesPerSecond = (int)Math.Round(this.framesSinceLastCheck / this.elapsedSinceLastCheck.TotalSeconds);
                int primitivesPerSecond = (int)Math.Round(this.primitivesSinceLastCheck / this.elapsedSinceLastCheck.TotalSeconds);
                int updatesPerSecond = (int)Math.Round(this.updatesSinceLastCheck / this.elapsedSinceLastCheck.TotalSeconds);

                this.elapsedSinceLastCheck = TimeSpan.Zero;
                this.drawsSinceLastCheck = 0;
                this.framesSinceLastCheck = 0;
                this.primitivesSinceLastCheck = 0;
                this.updatesSinceLastCheck = 0;

                // Update the debugging info to present to the outside world.
                this.DebugInfo = "\nFrames per second: " + framesPerSecond +
                    "\nDraw calls per frame: " + this.drawsPerFrame +
                    "\nDraw calls per second: " + drawsPerSecond +
                    "\nPrimitives per frame: " + this.primitivesPerFrame +
                    "\nPrimitives per second: " + primitivesPerSecond +
                    "\nUpdates per second: " + updatesPerSecond;
            }
        }
        #endregion
    }
}