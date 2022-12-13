using DefaultEcs;
using DefaultEcs.System;
using GameData.Structures.Contents.Terrains;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UOClient.Data;
using UOClient.ECS.Systems;
using UOClient.ECS.Systems.Renderers;
using UOClient.Maps.Components;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;

namespace UOClient
{
    public sealed class MainGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly IsometricCamera camera;
        private readonly World world;

        private GraphicsDevice device;
        private BasicEffect wireframeEffect;
        private StaticTextureFile staticsTextureFile;
        private TerrainTextureFile terrainsTextureFile;

        private SequentialSystem<GameTime> updateSystem;
        private SequentialSystem<GameTime> renderSystem;
        private SequentialSystem<GameTime> postRenderSystem;

        private readonly StaticData[] staticsData;
        private readonly TerrainData terrainData;

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

            using StaticsDataFile staticsDataFile = new();
            staticsData = staticsDataFile.Load(!Settings.UseEnhancedTextures);
            
            using TerrainsDataFile terrainsDataFile = new();
            terrainData = terrainsDataFile.Load();
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 1200;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "UO Client";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            staticsTextureFile = new(Settings.UseEnhancedTextures ? "ecTextures.bin" : "ccTextures.bin");
            terrainsTextureFile = new("terraintextures.bin");

            updateSystem = new SequentialSystem<GameTime>
            (
                new CameraSystem(world, camera),
                new TerrainLoaderSystem(world, device, new TerrainFile(1448, 1448), in terrainData),
                new StaticsLoaderSystem(world, device, new StaticsFile(1448, 1448), staticsTextureFile, staticsData)
            );

            renderSystem = new SequentialSystem<GameTime>
            (
                new TerrainRenderSystem(world, device, Content, camera, terrainsTextureFile, in terrainData),
                new StaticsRenderSystem(world, device, Content, camera)
            );

            postRenderSystem = new SequentialSystem<GameTime>
            (
                new BlocksSystem(world, 1448, 1448)
            );

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

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            if (!disposing)
                return;

            updateSystem.Dispose();
            renderSystem.Dispose();
            postRenderSystem.Dispose();

            device.Dispose();
            wireframeEffect.Dispose();
            staticsTextureFile.Dispose();
            graphics.Dispose();

            world.Dispose();
        }
    }
}