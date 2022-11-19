using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UOClient.Data;
using UOClient.ECS.Systems;
using UOClient.Maps.Components;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;

namespace UOClient
{
    public class MainGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly IsometricCamera camera;
        private readonly World world;

        private GraphicsDevice device;
        private BasicEffect wireframeEffect;

        private SequentialSystem<GameTime> updateSystem;
        private SequentialSystem<GameTime> renderSystem;
        private SequentialSystem<GameTime> postRenderSystem;

        private readonly StaticData[] staticsData;

        public MainGame()
        {
            graphics = new(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            camera = new();
            world = new World();

            staticsData = new StaticsDataFile().Load(!Settings.UseEnhancedTextures);
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 1200;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "UO Client";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            TextureFile textureFile = new(Settings.UseEnhancedTextures ? "ecTextures.bin" : "ccTextures.bin");

            updateSystem = new SequentialSystem<GameTime>
            (
                new CameraSystem(world, camera),
                new TerrainLoaderSystem(world, device, new TerrainFile(1448, 1448)),
                new StaticsLoaderSystem(world, device, new StaticsFile(1448, 1448), textureFile, staticsData)
            );

            renderSystem = new SequentialSystem<GameTime>
            (
                new TerrainRenderSystem(world, device, Content, camera),
                new StaticsRenderSystem(world, device, Content, camera)
            );

            postRenderSystem = new SequentialSystem<GameTime>
            (
                new BlocksSystem(world, 1448, 1448)
            );

            //var v = Content.Load<Texture2D>("statics/00003369");
            //TextureFile f = new("ecTextures.bin");
            //var v1 = new byte[128 * 64];
            //var v2 = f.ReadTexture(3369, out int w, out int h);

            //v.GetData(v1);

            //bool r = v1.SequenceEqual(v2);

            //Texture2D t = new(device, w, h, false, SurfaceFormat.Dxt5, 10);
            //t.SetData(0, 0, new Rectangle(0, 0, w, h), v2, 0, v2.Length);

            wireframeEffect = new(device);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            updateSystem.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            device.Clear(Color.DarkSlateBlue);
            //device.Clear(Color.Black);

            RasterizerState rs = new()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                FillMode = FillMode.Solid,
            };

            device.RasterizerState = rs;
            device.BlendState = BlendState.AlphaBlend;

            renderSystem.Update(gameTime);
            postRenderSystem.Update(gameTime);

            wireframeEffect.View = camera.ViewMatrix;
            wireframeEffect.Projection = camera.ProjectionMatrix;
            wireframeEffect.World = camera.WorldMatrix;

            RasterizerState rs2 = new()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            device.RasterizerState = rs2;

            foreach (EffectPass pass in wireframeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                camera.Test(device);
            }

            //Vector3 position = device.Viewport.Project(Vector3.Subtract(camera.Target, new Vector3(5, 5, 0)), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

            //spriteBatch.Begin();

            //fps.DrawFps(spriteBatch, font, new Vector2(position.X, position.Y), Color.MonoGameOrange);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}