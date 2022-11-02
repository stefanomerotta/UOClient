using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Structures;

namespace UOClient.Terrain
{
    internal class Terrain
    {
        private const int size = 3;
        private const int halfSize = size / 2;

        private readonly TerrainBlock?[,] blocks;
        private readonly Map map;
        private int blockX;
        private int blockY;
        private Bounds blockBounds;

        private readonly int blockMaxX;
        private readonly int blockMaxY;

        public Terrain(int width, int height)
        {
            blockX = -1;
            blockY = -1;

            blockMaxX = width / TerrainBlock.Size - 1;
            blockMaxY = height / TerrainBlock.Size - 1;
            map = new MyMap(width, height);

            blocks = new TerrainBlock[blockMaxX + 1, blockMaxY + 1];
        }

        private unsafe MapTile[] GetTiles(int blockX, int blockY)
        {
            MapTile[] tiles = new MapTile[TerrainBlock.VertexSize * TerrainBlock.VertexSize];
            map.FillChunk(blockX, blockY, tiles);

            return tiles;
        }

        public void OnLocationChanged(GraphicsDevice device, int newX, int newY)
        {
            int newBlockX = newX >> TerrainBlock.SizeOffset;
            int newBlockY = newY >> TerrainBlock.SizeOffset;

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

        public void Draw(GraphicsDevice device, IsometricCamera camera, GameTime gameTime, BasicArrayEffect effect, WaterEffect waterEffect)
        {
            DrawSolid(device, effect);
            DrawLiquid(device, waterEffect, camera.Target, gameTime);
        }

        private void DrawSolid(GraphicsDevice device, BasicArrayEffect effect)
        {
            EffectPass pass = effect.CurrentTechnique.Passes[0];

            for (int k = 1; k < (int)LandTileId.Water; k++)
            {
                ref SolidTerrainInfo info = ref SolidTerrainInfo.Values[k];

                effect.TextureIndex = k;
                effect.Texture0 = info.Texture0;
                effect.Texture1 = info.Texture1;
                effect.AlphaMask = info.AlphaMask;

                effect.Texture0Stretch = info.Texture0Stretch;
                effect.Texture1Stretch = info.Texture1Stretch;
                effect.AlphaMaskStretch = info.AlphaMaskStretch;

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

        private void DrawLiquid(GraphicsDevice device, WaterEffect effect, Vector3 target, GameTime gameTime)
        {
            EffectPass waterPass;

            for (int k = (int)LandTileId.Water; k < (int)LandTileId.Length; k++)
            {
                ref LiquidTerrainInfo info = ref LiquidTerrainInfo.Values[k];

                effect.TextureIndex = k;
                effect.Texture0 = info.Texture0;
                effect.Texture0Stretch = info.Texture0Stretch;

                effect.Normal = info.Normal;
                effect.NormalStretch = info.NormalStretch;

                effect.WaveHeight = info.WaveHeight;

                if (info.WindSpeed == 0)
                {
                    effect.WindForce = 0.05f;
                    effect.WindDirection = new(0, 1);
                }
                else
                {
                    effect.WindForce = info.WindSpeed;
                    effect.WindDirection = new(1, 0);
                }

                effect.Time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                effect.Center = new Vector2(target.X, target.Z);
                effect.FollowCenter = info.FollowCenter;

                waterPass = effect.CurrentTechnique.Passes[0];
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
    }
}
