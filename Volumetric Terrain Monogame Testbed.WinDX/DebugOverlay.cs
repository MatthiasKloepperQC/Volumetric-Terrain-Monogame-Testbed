using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Creates an overlay to show debugging information from <see cref="IDebugInfoComponent"/> instances.
    /// </summary>
    internal class DebugOverlay : IDrawableComponent, IUpdateableComponent
    {
        #region Fields
        /// <summary>
        /// The <see cref="componentManager"/> instance that contains the <see cref="IDebugInfoComponent"/> instances to show.
        /// </summary>
        private readonly ComponentManager componentManager;

        /// <summary>
        /// The <see cref="GraphicsDevice"/> instance to draw this <see cref="DebugOverlay"/> instance to.
        /// </summary>
        private readonly GraphicsDevice graphicsDevice;

        /// <summary>
        /// The margin around each overlayed rectangle in pixels.
        /// </summary>
        private readonly int margin = 10;

        /// <summary>
        /// The padding between the overlayed retangles and the rendered debug info text in pixels.
        /// </summary>
        private readonly int padding = 5;

        /// <summary>
        /// The texture used to render each overlayed rectangle.
        /// </summary>
        private readonly Texture2D rectangleTexture;

        /// <summary>
        /// A list that contains the <see cref="Rectangle"/> value as well as the debug info text to render for each overlayed rectangle.
        /// </summary>
        private readonly List<(Rectangle rectangle, string text)> rectangles;

        /// <summary>
        /// The <see cref="SpriteBatch"/> instance used to draw the overlayed rectangles.
        /// </summary>
        private readonly SpriteBatch spriteBatch;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="DebugOverlay"/> instance.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> instance to draw this <see cref="DebugOverlay"/> instance to.</param>
        /// <param name="manager">The <see cref="ComponentManager"/> instance that contains the <see cref="IDebugInfoComponent"/> instances to show.</param>
        /// <param name="font">The <see cref="SpriteFont"/> used to draw the debug information text.</param>
        internal DebugOverlay(GraphicsDevice graphicsDevice, ComponentManager manager, SpriteFont font)
        {
            // Check and set Parameters.
            this.componentManager = manager;
            this.DrawOrder = int.MaxValue; ;
            this.Enabled = true;
            this.Font = font;
            this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice), "The graphics device must be given.");
            this.rectangles = new List<(Rectangle rectangle, string text)>();
            this.spriteBatch = new SpriteBatch(this.graphicsDevice);
            this.UpdateOrder = int.MaxValue;

            // Set color (premultiplied alpha) for the overlay rectangles.
            Color debugBaseColor = Color.White;
            float debugColorAlpha = 51.0f / 255.0f;
            Color debugColor = debugBaseColor * debugColorAlpha;

            // Create texture base on the color.
            this.rectangleTexture = new Texture2D(this.graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            this.rectangleTexture.SetData(new Color[] { debugColor });
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the <see cref="SpriteFont"/> in which to render the debug info text.
        /// </summary>
        internal SpriteFont Font { get; set; }

        /// <summary>
        /// Gets or sets a value that defines at which point in the draw sequence this component is drawn.
        /// </summary>
        public int DrawOrder { get; set; }

        /// <summary>
        /// Gets or sets whether this instance should currently be used by other components.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value that defines at which point in the update sequence this component is updated.
        /// </summary>
        public int UpdateOrder { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Called when the game determines it is time to draw a frame with this component.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Draw(GameTime)"/>.</param>
        public void Draw(GameTime gameTime)
        {
            // The use of SpriteBatch changes some stati of the graphics device:
            // http://rbwhitaker.wikidot.com/drawing-in-wireframe-mode
            // http://www.shawnhargreaves.com/blog/spritebatch-and-renderstates-in-xna-game-studio-4-0.html
            // Save those stati for later restoration.
            BlendState backupBlendState = this.graphicsDevice.BlendState;
            DepthStencilState backupDepthStencilState = this.graphicsDevice.DepthStencilState;
            RasterizerState backupRasterizerState = this.graphicsDevice.RasterizerState;
            SamplerState backupSamplerState = this.graphicsDevice.SamplerStates[0];

            // Draw all overlay rectangles.
            this.spriteBatch.Begin();
            foreach ((Rectangle Rectangle, string Text) overlayRectangle in this.rectangles)
            {
                this.spriteBatch.Draw(this.rectangleTexture, overlayRectangle.Rectangle, Color.White);
                this.spriteBatch.DrawString(this.Font, overlayRectangle.Text, new Vector2(overlayRectangle.Rectangle.X + this.padding, overlayRectangle.Rectangle.Y + this.padding), Color.Black);
            }
            this.spriteBatch.End();

            // Restore all relevant stati of the graphics device.
            this.graphicsDevice.BlendState = backupBlendState;
            this.graphicsDevice.DepthStencilState = backupDepthStencilState;
            this.graphicsDevice.RasterizerState = backupRasterizerState;
            this.graphicsDevice.SamplerStates[0] = backupSamplerState;
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
        /// Called when the game determines to update the state of this instance.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to <see cref="Update(GameTime)"/>.</param>
        public void Update(GameTime gameTime)
        {
            // Clear list of overlay rectangles and texts.
            this.rectangles.Clear();

            // Setup rectangles with the right minimum size for each text.
            Rectangle lastRectangle = new Rectangle(0, 0, 0, 0);
            int doublePadding = this.padding * 2;
            int maxWidth = 0;
            string debugText = string.Empty;
            foreach (IDebugInfoComponent debugComponent in this.componentManager.GetComponentsByInterface<IDebugInfoComponent>().FindAll(component => component.Enabled && component.DebugOutputEnabled))
            {
                // Create and add new overlay rectangle to list.
                debugText = debugComponent.Description + "\n" + debugComponent.DebugInfo;
                Vector2 textDimensions = this.Font.MeasureString(debugText);
                lastRectangle = new Rectangle(this.margin, lastRectangle.Y + lastRectangle.Height + this.margin,
                    (int)Math.Ceiling(textDimensions.X) + doublePadding, (int)Math.Ceiling(textDimensions.Y) + doublePadding);
                this.rectangles.Add((lastRectangle, debugText));
                maxWidth = Math.Max(maxWidth, lastRectangle.Width);
            }

            // Change width of all rectangles to the maximum width determined during the last step.
            for (int overlayRectangleIndex = 0; overlayRectangleIndex < this.rectangles.Count; overlayRectangleIndex++)
            {
                (Rectangle rectangle, string text) overlayRectangle = this.rectangles[overlayRectangleIndex];
                overlayRectangle.rectangle.Width = maxWidth;
                this.rectangles[overlayRectangleIndex] = overlayRectangle;
            }
        }
        #endregion
    }
}