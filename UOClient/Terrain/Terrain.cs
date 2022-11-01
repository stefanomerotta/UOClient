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
        private const int outerSize = 5;
        private const int innerSize = outerSize - 2;
        private const int halfInnerSize = innerSize / 2;
        private const int halfOuterSize = outerSize / 2;

        private readonly TerrainBlock?[,] blocks;
        private readonly Map map;
        private int blockX;
        private int blockY;

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

        public void Load(GraphicsDevice device, int x, int y)
        {
            OnLocationChanged(device, x, y);
        }

        public void OnLocationChanged(GraphicsDevice device, int newX, int newY)
        {
            int newBlockX = newX >> TerrainBlock.SizeOffset;
            int newBlockY = newY >> TerrainBlock.SizeOffset;

            if (blockX == newBlockX && blockY == newBlockY)
                return;

            blockX = newBlockX;
            blockY = newBlockY;

            LoadBlocks(device);
            UnloadUnusedBlocks();
        }

        private void LoadBlocks(GraphicsDevice device)
        {
            int startX = Math.Clamp(blockX - halfInnerSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfInnerSize, 0, blockMaxY);
            int endX = Math.Clamp(blockX + halfInnerSize, 0, blockMaxX);
            int endY = Math.Clamp(blockY + halfInnerSize, 0, blockMaxY);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    blocks[x, y] ??= new(device, x, y, GetTiles(x, y));
                }
            }
        }

        private void UnloadUnusedBlocks()
        {
            int startX = blockX - halfOuterSize;
            int startY = blockY + halfOuterSize;
            int endX = blockX - halfOuterSize;
            int endY = blockY + halfOuterSize;

            for (int k = 0; k < outerSize; k++)
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
            int startX = Math.Clamp(blockX - halfInnerSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfInnerSize, 0, blockMaxY);
            int endX = Math.Clamp(blockX + halfInnerSize, 0, blockMaxX);
            int endY = Math.Clamp(blockY + halfInnerSize, 0, blockMaxY);

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

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        blocks[x, y]!.Draw(device, k);
                    }
                }
            }

            EffectPass waterPass;

            for (int k = (int)LandTileId.Water; k < (int)LandTileId.Length; k++)
            {
                ref LiquidTerrainInfo info = ref LiquidTerrainInfo.Values[k];

                waterEffect.TextureIndex = k;
                waterEffect.Texture0 = info.Texture0;
                waterEffect.Texture0Stretch = info.Texture0Stretch;

                waterEffect.Normal = info.Normal;
                waterEffect.NormalStretch = info.NormalStretch;

                waterEffect.WaveHeight = info.WaveHeight;

                if (info.WindSpeed == 0)
                {
                    waterEffect.WindForce = 0.05f;
                    waterEffect.WindDirection = new(0, 1);
                }
                else
                {
                    waterEffect.WindForce = info.WindSpeed;
                    waterEffect.WindDirection = new(1, 0);
                }

                waterEffect.Time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                waterEffect.Center = new Vector2(camera.Target.X, camera.Target.Z);
                waterEffect.FollowCenter = info.FollowCenter;

                waterPass = waterEffect.CurrentTechnique.Passes[0];
                waterPass.Apply();

                for (int i = startX; i <= endX; i++)
                {
                    for (int j = startY; j <= endY; j++)
                    {
                        blocks[i, j]!.Draw(device, k);
                    }
                }
            }
        }

        public void DrawBoundaries(GraphicsDevice device)
        {
            int startX = Math.Clamp(blockX - halfInnerSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfInnerSize, 0, blockMaxY);

            for (int i = startX; i < startX + innerSize && i <= blockMaxX; i++)
            {
                for (int j = startY; j < startY + innerSize && j <= blockMaxY; j++)
                {
                    blocks[i, j].DrawBoundaries(device);
                }
            }
        }
    }
}
