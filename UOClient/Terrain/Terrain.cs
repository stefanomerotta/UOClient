using Microsoft.Xna.Framework.Graphics;
using SharpDX.WIC;
using System;
using System.Runtime.InteropServices;
using UOClient.Data;
using UOClient.Structures;

namespace UOClient.Terrain
{
    internal class Terrain
    {
        private const int size = 3;
        private const int halfSize = size / 2;

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

        private unsafe MapTile[,] GetHeights(int blockX, int blockY)
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

        public void Load(int x, int y)
        {
            OnLocationChanged(x, y);
        }

        public void OnLocationChanged(int newX, int newY)
        {
            blockX = newX / TerrainBlock.Size;
            blockY = newY / TerrainBlock.Size;

            int startX = Math.Clamp(blockX - halfSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfSize, 0, blockMaxY);

            for (int j = startY; j < startY + size && j <= blockMaxY; j++)
            {
                for (int i = startX; i < startX + size && i <= blockMaxX; i++)
                {
                    blocks[i, j] ??= new(i, j, GetHeights(i, j));
                }
            }
        }

        public void Draw(GraphicsDevice device)
        {
            int startX = Math.Clamp(blockX - halfSize, 0, blockMaxX);
            int startY = Math.Clamp(blockY - halfSize, 0, blockMaxY);

            for (int i = startX; i < startX + size && i <= blockMaxX; i++)
            {
                for (int j = startY; j < startY + size && j <= blockMaxY; j++)
                {
                    blocks[i, j].Draw(device);
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
