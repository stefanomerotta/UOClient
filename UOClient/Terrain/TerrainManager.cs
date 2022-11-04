using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Structures;

namespace UOClient.Terrain
{
    internal class TerrainManager
    {
        private const int size = 3;
        private const int halfSize = size / 2;

        private readonly TerrainBlock?[,] blocks;
        private readonly Map map;
        private int blockX;
        private int blockY;
        private Bounds blockBounds;

        private GraphicsDevice device;
        private IsometricCamera camera;
        private BasicArrayEffect solid;
        private WaterEffect liquid;

        private readonly int blockMaxX;
        private readonly int blockMaxY;

        public TerrainManager(int width, int height)
        {
            blockX = -1;
            blockY = -1;

            blockMaxX = width / TerrainBlock.Size - 1;
            blockMaxY = height / TerrainBlock.Size - 1;
            map = new MyMap(width, height);

            blocks = new TerrainBlock[blockMaxX + 1, blockMaxY + 1];
        }

        public void Initialize(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.device = device;
            this.camera = camera;

            solid = new(contentManager)
            {
                TextureEnabled = true,
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                World = camera.WorldMatrix,
                GridEnabled = true
            };

            liquid = new(contentManager)
            {
                TextureEnabled = true,
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                World = camera.WorldMatrix,
            };
        }

        public void OnLocationChanged()
        {
            UpdateMatrices();

            Vector3 target = camera.Target;

            int newBlockX = (int)target.X >> TerrainBlock.SizeOffset;
            int newBlockY = (int)target.Z >> TerrainBlock.SizeOffset;

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

            LoadBlocks(device);
            UnloadUnusedBlocks();
        }

        private void LoadBlocks(GraphicsDevice device)
        {
            for (int y = blockBounds.StartY; y < blockBounds.EndY; y++)
            {
                for (int x = blockBounds.StartX; x < blockBounds.EndX; x++)
                {
                    blocks[x, y] ??= new(device, x, y, GetTiles(x, y));
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

                ref TerrainBlock? block = ref blocks[x, y];
                if (block is null)
                    return;

                block.Dispose();
                block = null;
            }
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
                        blocks[x, y]!.Draw(device, k);
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
                        blocks[i, j]!.Draw(device, k);
                    }
                }
            }
        }

        public void DrawBoundaries(GraphicsDevice device)
        {
            int startX = Math.Clamp(blockX - halfSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfSize, 0, blockMaxY);

            for (int i = startX; i < startX + size && i <= blockMaxX; i++)
            {
                for (int j = startY; j < startY + size && j <= blockMaxY; j++)
                {
                    blocks[i, j].DrawBoundaries(device);
                }
            }
        }

        private MapTile[] GetTiles(int blockX, int blockY)
        {
            MapTile[] tiles = new MapTile[TerrainBlock.VertexSize * TerrainBlock.VertexSize];
            map.FillChunk(blockX, blockY, tiles);

            return tiles;
        }
    }
}
