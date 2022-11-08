using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Maps.Components;
using UOClient.Maps.Terrain;

namespace UOClient.Maps
{
    internal class MapManager
    {
        private const int size = 3;
        private const int halfSize = size / 2;

        private readonly MapBlock?[,] blocks;
        private readonly MapFile map;
        private int blockX;
        private int blockY;
        private Bounds blockBounds;

        private GraphicsDevice device;
        private IsometricCamera camera;
        private BasicArrayEffect solid;
        private WaterEffect liquid;
        private StaticsEffect statics;

        private readonly int blockMaxX;
        private readonly int blockMaxY;
        private readonly StaticData[] staticsData;

        public MapManager(int width, int height)
        {
            blockX = -1;
            blockY = -1;

            map = new MapFile(width, height);

            blockMaxX = map.BlocksWidth - 1;
            blockMaxY = map.BlocksHeight - 1;

            blocks = new MapBlock[map.BlocksWidth, map.BlocksHeight];
            
            using StaticsDataFile staticsDataFile = new();
            staticsData = staticsDataFile.Load(false);
        }

        public void Initialize(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.device = device;
            this.camera = camera;

            SolidTerrainInfo.Load(contentManager);
            LiquidTerrainInfo.Load(contentManager);

            solid = new(contentManager)
            {
                TextureEnabled = true,
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                World = camera.WorldMatrix,
                //GridEnabled = true
            };

            liquid = new(contentManager)
            {
                TextureEnabled = true,
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                World = camera.WorldMatrix,
            };

            statics = new(contentManager)
            {
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                World = camera.WorldMatrix,
            };
        }

        public void OnLocationChanged()
        {
            UpdateMatrices();

            Vector3 target = camera.Target;

            int newBlockX = (int)target.X >> MapFile.BlockSizeShift;
            int newBlockY = (int)target.Z >> MapFile.BlockSizeShift;

            if (blockX == newBlockX && blockY == newBlockY)
                return;

            blockX = newBlockX;
            blockY = newBlockY;

            blockBounds = new
            (
                Math.Clamp(blockX - halfSize, 0, blockMaxX),
                Math.Clamp(blockY - halfSize, 0, blockMaxY),
                Math.Clamp(blockX + halfSize + 1, 0, blockMaxX),
                Math.Clamp(blockY + halfSize + 1, 0, blockMaxY)
            );

            LoadBlocks();
            UnloadUnusedBlocks();
        }

        private void UpdateMatrices()
        {
            solid.View = camera.ViewMatrix;
            liquid.View = camera.ViewMatrix;
        }

        public void Draw(GameTime gameTime)
        {
            solid.PreDraw();
            liquid.PreDraw();
            
            DrawSolid();
            DrawLiquid(gameTime);
            DrawStatics();
        }

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

                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
                    {
                        blocks[x, y]!.Terrain.Draw(device, k);
                    }
                }
            }
        }

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

                for (int i = blockBounds.StartX; i < blockBounds.EndX; i++)
                {
                    for (int j = blockBounds.StartY; j < blockBounds.EndY; j++)
                    {
                        blocks[i, j]!.Terrain.Draw(device, k);
                    }
                }
            }
        }

        private void DrawStatics()
        {
            EffectPass pass = statics.CurrentTechnique.Passes[0];

            pass.Apply();

            for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
            {
                for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
                {
                    blocks[x, y]!.Statics.Draw(device);
                }
            }
        }

        private void LoadBlocks()
        {
            for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
            {
                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    ref MapBlock? block = ref blocks[x, y];

                    if (block is null)
                    {
                        block = MapBlock.Pool.Get();
                        block.Initialize(device, x, y, map, staticsData);
                    }
                }
            }
        }

        private void UnloadUnusedBlocks()
        {
            int startX = blockBounds.StartX - 1;
            int startY = blockBounds.StartY - 1;
            int endX = blockBounds.EndX + 1;
            int endY = blockBounds.EndY + 1;

            for (int k = 0; k < size + 2; k++)
            {
                Unload(startX + k, startY);
                Unload(startX + k, endY);
                Unload(startX, startY + k);
                Unload(endX, startY + k);
            }

            void Unload(int x, int y)
            {
                if (x < 0 || x > blockMaxX)
                    return;

                if (y < 0 || y > blockMaxY)
                    return;

                ref MapBlock? block = ref blocks[x, y];
                if (block is null)
                    return;

                MapBlock.Pool.Return(block);
                block = null;
            }
        }
    }
}
