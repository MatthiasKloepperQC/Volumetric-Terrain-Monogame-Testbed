using BattletechUniverse.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    public class Game1 : Game
    {
        private FreeCamera camera;
        private ComponentManager componentManager;
        private DebugOverlay debugOverlay;
        private readonly GraphicsDeviceManager graphics;
        private VolumeRaycastingEffect vrce;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);

            // Hook into PreparingDeviceSettings event to change e.g. the graphics profile.
            this.graphics.PreparingDeviceSettings += this.Graphics_PreparingDeviceSettings;
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;

            // Set viewport to 1000 x 1000 pixels.
            this.graphics.PreferredBackBufferHeight = 1000;
            this.graphics.PreferredBackBufferWidth = 1000;
            this.graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.componentManager = new ComponentManager();
            this.vrce = new VolumeRaycastingEffect(this.GraphicsDevice);

            // max Size for Texture2D on this card: 16.384 x 16.384 * 32 Bit
            // max Size for Texture3D on this card: 512 * 512 * 512 * 32 Bit

            // Häufigkeit des Update()-Aufrufs anpassen.
            this.IsFixedTimeStep = false;

            // V-Sync deaktivieren.
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.graphics.ApplyChanges();

            // Initialize a basic camera (Pos: (0.0, 0.0, 0.0); Viewing direction (0.0, 0.0, -1.0); Up direction (0.0, 1.0, 0.0)
            this.camera = new FreeCamera(this.GraphicsDevice);

            // Initialize basic debug overlay.
            this.debugOverlay = new DebugOverlay(this.GraphicsDevice, this.componentManager, null);
            this.componentManager.RegisterComponent(this.debugOverlay);

            // Initialize debugging components.
            DebugInfoGraphicsPerformance debugInfoGraphicsPerformance = new DebugInfoGraphicsPerformance(this.GraphicsDevice)
            {
                DebugOrder = 0,
                DrawOrder = this.debugOverlay.DrawOrder - 1,
                UpdateOrder = this.debugOverlay.UpdateOrder - 1
            };
            this.componentManager.RegisterComponent(debugInfoGraphicsPerformance);
            DebugInfoSoftwareEnvironment debugInfoSoftwareEnvironment = new DebugInfoSoftwareEnvironment
            {
                DebugOrder = 2
            };
            this.componentManager.RegisterComponent(debugInfoSoftwareEnvironment);
            DebugInfoGraphicsEnvironment debugInfoGraphicsEnvironment = new DebugInfoGraphicsEnvironment(this.GraphicsDevice)
            {
                DebugOrder = 1,
                UpdateOrder = this.debugOverlay.UpdateOrder - 1
            };
            this.componentManager.RegisterComponent(debugInfoGraphicsEnvironment);

            base.Initialize();
        }

        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Switch to highest available graphics profile.
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
            
            // FullScreen?
            bool fullScreen = false;
            if (fullScreen)
            {
                e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Height;
                e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = e.GraphicsDeviceInformation.Adapter.CurrentDisplayMode.Width;
                e.GraphicsDeviceInformation.PresentationParameters.HardwareModeSwitch = true;
                e.GraphicsDeviceInformation.PresentationParameters.IsFullScreen = true;
            }
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            this.debugOverlay.Font = this.Content.Load<SpriteFont>("Calibri 8 regular");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // TODO: Add your update logic here
            // Update all updateable components.
            foreach (IUpdateableComponent updateableComponent in this.componentManager.GetComponentsByInterface<IUpdateableComponent>().FindAll(component => component.Enabled))
            {
                updateableComponent.Update(gameTime);
            }

            this.vrce.CameraPositionWorldSpace = Vector3.Zero;
            this.vrce.WorldMatrix = Matrix.Identity;
            this.vrce.ProjectionMatrix = this.camera.ProjectionMatrix;
            this.vrce.ViewMatrix = this.camera.ViewMatrix;
            this.vrce.Mode = EffectMode.ColorFromCameraRayDirection;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            // Alle vorgesehenen Durchläufe des Effektes durchgehen.
            for (int passIndex = 0; passIndex < this.vrce.CurrentTechnique.Passes.Count; passIndex++)
            {
                this.vrce.CurrentTechnique.Passes[passIndex].Apply();

                //this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                this.vrce.DrawFullScreenQuad();
            }

            // Draw all drawable components.
            foreach (IDrawableComponent drawableComponent in this.componentManager.GetComponentsByInterface<IDrawableComponent>().FindAll(component => component.Enabled))
            {
                drawableComponent.Draw(gameTime);
            }

            base.Draw(gameTime);
        }
    }
}