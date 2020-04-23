using BattletechUniverse.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    public class Game1 : Game
    {
        private FreeCamera camera;
        private readonly GraphicsDeviceManager graphics;
        private VertexBuffer vbuffer;
        private VolumeRaycastingEffect vrce;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);

            // Hook into PreparingDeviceSettings event to change e.g. the graphics profile.
            this.graphics.PreparingDeviceSettings += this.Graphics_PreparingDeviceSettings;
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.vrce = new VolumeRaycastingEffect(this.GraphicsDevice);

            // max Size for Texture2D on this card: 16.384 x 16.384 * 32 Bit
            // max Size for Texture3D on this card: 512 * 512 * 512 * 32 Bit

            // Initialize a basic camera (Pos: (0.0, 0.0, 0.0); Viewing direction (0.0, 0.0, -1.0); Up direction (0.0, 1.0, 0.0)
            this.camera = new FreeCamera(this.GraphicsDevice);

            base.Initialize();
        }

        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Switch to highest available graphics profile.
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // TODO: Add your update logic here

            this.vrce.UseVertexColor = true;
            this.vrce.WorldMatrix = Matrix.Identity;
            this.vrce.ProjectionMatrix = this.camera.ProjectionMatrix;
            this.vrce.ViewMatrix = this.camera.ViewMatrix;

            this.vbuffer = new VertexBuffer(this.GraphicsDevice, VertexPositionColor.VertexDeclaration, 6, BufferUsage.WriteOnly);
            VertexPositionColor[] vdata = new VertexPositionColor[6];
            vdata[0] = new VertexPositionColor(new Vector3(-2, 2, -5), Color.Red);
            vdata[1] = new VertexPositionColor(new Vector3(2, 2, -5), Color.Green);
            vdata[2] = new VertexPositionColor(new Vector3(-2, -2, -5), Color.Blue);
            vdata[3] = new VertexPositionColor(new Vector3(-2, -2, -5), Color.Yellow);
            vdata[4] = new VertexPositionColor(new Vector3(2, 2, -5), Color.Orange);
            vdata[5] = new VertexPositionColor(new Vector3(2, -2, -5), Color.Purple);
            this.vbuffer.SetData(vdata);
            this.GraphicsDevice.SetVertexBuffer(this.vbuffer);
            RasterizerState nrs = new RasterizerState();
            nrs.CullMode = CullMode.None;
            //this.GraphicsDevice.RasterizerState = nrs;
            this.vrce.UseRaymarchFullScreen = true;

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

                this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}