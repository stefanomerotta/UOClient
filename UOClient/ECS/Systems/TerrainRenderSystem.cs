using DefaultEcs;
using DefaultEcs.System;
using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using UOClient.Effects;
using UOClient.Maps.Terrain;

namespace UOClient.ECS.Systems
{
    internal sealed class TerrainRenderSystem : ISystem<GameTime>
    {
        private readonly GraphicsDevice device;
        private readonly EntityMap<TerrainBlock> activeBlocks;

        private readonly SolidTerrainEffect solid;
        private readonly LiquidTerrainEffect liquid;
        private readonly IsometricCamera camera;

        public bool IsEnabled { get; set; }

        public TerrainRenderSystem(World world, GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.camera = camera;
            this.device = device;

            activeBlocks = world.GetEntities()
                .With<TerrainBlock>()
                .AsMap<TerrainBlock>();

            SolidTerrainInfo.Load(contentManager);
            LiquidTerrainInfo.Load(contentManager);

            ref readonly Matrix worldViewProjection = ref camera.WorldViewProjection;

            solid = new(contentManager, in worldViewProjection);
            liquid = new(contentManager, in worldViewProjection);
        }

        public void Update(GameTime state)
        {
            solid.SetWorldViewProjection(in camera.WorldViewProjection);
            liquid.SetWorldViewProjection(in camera.WorldViewProjection);

            DrawSolid();
            DrawLiquid(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawSolid()
        {
            EffectPass pass = solid.CurrentTechnique.Passes[0];

            for (int k = 1; k < (int)LandTileId.Water; k++)
            {
                ref SolidTerrainInfo info = ref SolidTerrainInfo.Values[k];

                solid.TextureIndex = k;
                solid.Texture0 = info.Texture0;
                solid.Texture1 = info.Texture1;
                solid.AlphaMask = info.AlphaMask;

                solid.Texture0Stretch = info.Texture0Stretch;
                solid.Texture1Stretch = info.Texture1Stretch;
                solid.AlphaMaskStretch = info.AlphaMaskStretch;

                pass.Apply();

                foreach (TerrainBlock block in activeBlocks.Keys)
                {
                    block.Draw(device, k);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawLiquid(GameTime gameTime)
        {
            EffectPass waterPass;
            Vector3 target = camera.Target;

            liquid.Time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            liquid.Center = new Vector2(target.X, target.Z);

            for (int k = (int)LandTileId.Water; k < (int)LandTileId.Length; k++)
            {
                ref LiquidTerrainInfo info = ref LiquidTerrainInfo.Values[k];

                liquid.TextureIndex = k;
                liquid.Texture0 = info.Texture0;
                liquid.Texture0Stretch = info.Texture0Stretch;

                liquid.Normal = info.Normal;
                liquid.NormalStretch = info.NormalStretch;

                liquid.WaveHeight = info.WaveHeight;

                if (info.WindSpeed == 0)
                {
                    liquid.WindForce = 0.05f;
                    liquid.WindDirection = new(0, 1);
                }
                else
                {
                    liquid.WindForce = info.WindSpeed;
                    liquid.WindDirection = new(1, 0);
                }

                liquid.FollowCenter = info.FollowCenter;

                waterPass = liquid.CurrentTechnique.Passes[0];
                waterPass.Apply();

                foreach (TerrainBlock block in activeBlocks.Keys)
                {
                    block.Draw(device, k);
                }
            }
        }

        public void Dispose()
        { 
            foreach(TerrainBlock block in activeBlocks.Keys)
            {
                block.Dispose();
            }
        }
    }
}
