using GameData.Enums;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Structures;

namespace UOClient.Terrain
{
    internal class Terrain
    {
        private const int size = 3;
        private const int halfSize = size / 2;

        private static readonly Texture2D[] textures = new Texture2D[(int)LandTileId.Length];

        private readonly TerrainBlock[,] blocks;
        private readonly Map map;
        private int blockX;
        private int blockY;

        private readonly int blockMaxX;
        private readonly int blockMaxY;

        public Terrain(int id, int width, int height)
        {
            blockMaxX = width / TerrainBlock.Size - 1;
            blockMaxY = height / TerrainBlock.Size - 1;
            map = new MyMap(id, width, height);

            blocks = new TerrainBlock[width / TerrainBlock.Size, height / TerrainBlock.Size];
        }

        private unsafe MapTile[,] GetTiles(int blockX, int blockY)
        {
            MapTile[,] toRet = new MapTile[TerrainBlock.VertexSize, TerrainBlock.VertexSize];

            Span<byte> rawTiles = stackalloc byte[TerrainBlock.VertexSize * TerrainBlock.VertexSize * sizeof(MapTile)];
            Span<MapTile> tiles = MemoryMarshal.Cast<byte, MapTile>(rawTiles);
            map.FillChunk(blockX, blockY, tiles);

            for (int i = 0; i < tiles.Length; i++)
            {
                toRet[i % TerrainBlock.VertexSize, i / TerrainBlock.VertexSize] = tiles[i];
            }

            return toRet;
        }

        public void Load(GraphicsDevice device, int x, int y)
        {
            OnLocationChanged(device, x, y);
        }

        public void OnLocationChanged(GraphicsDevice device, int newX, int newY)
        {
            blockX = newX / TerrainBlock.Size;
            blockY = newY / TerrainBlock.Size;

            int startX = Math.Clamp(blockX - halfSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfSize, 0, blockMaxY);

            for (int j = startY; j < startY + size && j <= blockMaxY; j++)
            {
                for (int i = startX; i < startX + size && i <= blockMaxX; i++)
                {
                    blocks[i, j] ??= new(device, i, j, GetTiles(i, j));
                }
            }
        }

        public void Draw(GraphicsDevice device, BasicArrayEffect effect, WaterEffect waterEffect)
        {
            int startX = Math.Clamp(blockX - halfSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfSize, 0, blockMaxY);
            EffectPass pass = effect.CurrentTechnique.Passes[0];

            for (int k = 1; k < (int)LandTileId.Length; k++)
            {
                ref TerrainInfo info = ref TerrainInfo.Values[k];

                effect.TextureIndex = k;
                effect.Texture0 = info.Texture0;
                effect.Texture1 = info.Texture1;
                effect.AlphaMask = info.Texture2;

                effect.Texture0Stretch = info.Texture0Stretch;
                effect.Texture1Stretch = info.Texture1Stretch;
                effect.AlphaMaskStretch = info.Texture2Stretch;

                pass.Apply();

                for (int i = startX; i < startX + size && i <= blockMaxX; i++)
                {
                    for (int j = startY; j < startY + size && j <= blockMaxY; j++)
                    {
                        blocks[i, j].Draw(device, k);
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

        public static void LoadTextures(ContentManager contentManager)
        {
            Set(LandTileId.Dirt, "02000020_Dirt_A");
            Set(LandTileId.Forest, "02000040_Forest_A");
            Set(LandTileId.Grass, "02000010_Grass_C");
            Set(LandTileId.Jungle, "02000030_Jungle_A");
            Set(LandTileId.Lava, "02000100_Lava_A");
            Set(LandTileId.Rock, "02000060_Rock_A");
            Set(LandTileId.Sand, "02000070_Sand_A");
            Set(LandTileId.Snow, "02000080_Snow_A");
            Set(LandTileId.Swamp, "02000700_Swamp_Water_A");
            Set(LandTileId.Unused, "02000000_Black_Void_A");
            Set(LandTileId.Water, "02000051_water");
            Set(LandTileId.Acid, "02000490_Acid_A");

            void Set(LandTileId id, string landTexture)
            {
                textures[(int)id] = contentManager.Load<Texture2D>($"land/{landTexture}");
            }
        }
    }
}
