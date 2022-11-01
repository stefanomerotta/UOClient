using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UOClient.Effects;
using UOClient.Terrain;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using Map = UOClient.Terrain.Terrain;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace UOClient
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private readonly IsometricCamera camera;
        private readonly Map terrain;
        private SpriteBatch spriteBatch;
        private readonly FPSCounter fps;
        private SpriteFont font;

        private BasicEffect wireframeEffect;
        private BasicArrayEffect effect;
        private WaterEffect waterEffect;

        public Game1()
        {
            graphics = new(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            terrain = new(1448, 1448);
            camera = new();
            fps = new();
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 1000;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Riemer's MonoGame Tutorials -- 3D Series 1";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            effect = new(Content) { TextureEnabled = true, };
            waterEffect = new(Content) { TextureEnabled = true };
            wireframeEffect = new(device) { VertexColorEnabled = true };

            spriteBatch = new(device);
            font = Content.Load<SpriteFont>("fonts/File");

            SolidTerrainInfo.Load(Content);
            LiquidTerrainInfo.Load(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            camera.HandleKeyboardInput();
            terrain.OnLocationChanged(device, (int)camera.Target.X, (int)camera.Target.Z);
            fps.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            device.Clear(Color.DarkSlateBlue);
            //device.Clear(Color.Black);

            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;
            effect.World = camera.WorldMatrix;

            waterEffect.View = camera.ViewMatrix;
            waterEffect.Projection = camera.ProjectionMatrix;
            waterEffect.World = camera.WorldMatrix;

            wireframeEffect.View = camera.ViewMatrix;
            wireframeEffect.Projection = camera.ProjectionMatrix;
            wireframeEffect.World = camera.WorldMatrix;

            RasterizerState rs = new()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                FillMode = FillMode.Solid,
            };

            device.RasterizerState = rs;
            device.BlendState = BlendState.AlphaBlend;

            waterEffect.PreDraw();
            effect.PreDraw();

            terrain.Draw(device, camera, gameTime, effect, waterEffect);

            RasterizerState rs2 = new()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            //device.RasterizerState = rs2;

            //foreach (EffectPass pass in wireframeEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    camera.Test(device);
            //    terrain.DrawBoundaries(device);
            //}

            //Vector3 position = device.Viewport.Project(Vector3.Subtract(camera.Target, new Vector3(5, 5, 0)), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

            //spriteBatch.Begin();

            //fps.DrawFps(spriteBatch, font, new Vector2(position.X, position.Y), Color.MonoGameOrange);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}