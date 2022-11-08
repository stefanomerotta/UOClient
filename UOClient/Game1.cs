﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UOClient.Maps;
//using UOClient.Terrain;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;

namespace UOClient
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly IsometricCamera camera;
        private readonly MapManager map;

        private GraphicsDevice device;
        private BasicEffect wireframeEffect;

        public Game1()
        {
            graphics = new(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            map = new(1448, 1448);
            camera = new();
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 1200;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Riemer's MonoGame Tutorials -- 3D Series 1";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            Globals.Device = device;

            wireframeEffect = new(device) { VertexColorEnabled = true };

            map.Initialize(device, Content, camera);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (camera.HandleKeyboardInput()) ;
            //{
            map.OnLocationChanged();
            //}

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            device.Clear(Color.DarkSlateBlue);
            //device.Clear(Color.Black);

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

            map.Draw(gameTime);

            //RasterizerState rs2 = new()
            //{
            //    CullMode = CullMode.None,
            //    FillMode = FillMode.WireFrame
            //};

            //device.RasterizerState = rs2;

            //foreach (EffectPass pass in wireframeEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    camera.Test(device);
            //}

            //Vector3 position = device.Viewport.Project(Vector3.Subtract(camera.Target, new Vector3(5, 5, 0)), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

            //spriteBatch.Begin();

            //fps.DrawFps(spriteBatch, font, new Vector2(position.X, position.Y), Color.MonoGameOrange);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}