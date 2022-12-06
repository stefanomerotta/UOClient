using FileConverter.CC.Structures;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NewMapTile = FileConverter.Structures.TerrainTile;

namespace FileConverter.CC
{
    internal sealed class TerrainConverter : IDisposable
    {
        private const int oldSize = 8;
        private const int oldSizeShift = 3;
        
        private readonly int newSize;
        private readonly int newSizeShift;
        private readonly int deltaSizeShift;
        private readonly int deltaSize;

        private readonly int width;
        private readonly int height;
        private readonly int oldChunkWidth;
        private readonly int oldChunkHeight;
        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly string path;
        private readonly UOPMap map;

        public TerrainConverter(string path, int id, int width, int height, int newChunkSize)
        {
            this.path = path;
            this.width = width;
            this.height = height;

            newSize = newChunkSize;
            newSizeShift = (int)Math.Log2(newChunkSize);
            deltaSize = newSize / oldSize;
            deltaSizeShift = (int)Math.Log2(deltaSize);

            oldChunkWidth = width >> oldSizeShift;
            oldChunkHeight = height >> oldSizeShift;

            newChunkWidth = (int)Math.Ceiling(width / (double)newSize);
            newChunkHeight = (int)Math.Ceiling(height / (double)newSize);

            map = new(path, id, width, height);
        }

        public unsafe void ConvertChunk(int x, int y, Span<NewMapTile> chunk)
        {
            Debug.Assert(chunk.Length == (newSize + 1) * (newSize + 1));

            int oldX = x << deltaSizeShift;
            int oldY = y << deltaSizeShift;

            Span<TerrainTile> newChunk = MemoryMarshal.Cast<NewMapTile, TerrainTile>(chunk);

            LoadOldChunks(oldX, oldY, newChunk);
            LoadRightDelimiters(oldX, oldY, newChunk);
            LoadBottomDelimiters(oldX, oldY, newChunk);
            LoadCornerDelimiter(oldX, oldY, newChunk);
        }

        private unsafe void LoadOldChunks(int startX, int startY, Span<TerrainTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            Span<TerrainTile> oldChunk = stackalloc TerrainTile[oldSize * oldSize];

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;
                int tileX = x << oldSizeShift;

                for (int y = 0; y < deltaSize; y++)
                {
                    int oldY = startY + y;
                    int tileY = y << oldSizeShift;

                    LoadOldChunk(oldX, oldY, oldChunk);

                    for (int k = 0; k < oldSize; k++)
                    {
                        oldChunk.Slice(k * oldSize, oldSize).CopyTo(tiles.Slice(tileX + (tileY + k) * (newSize + 1), oldSize));
                    }
                }
            }
        }

        private unsafe void LoadRightDelimiters(int startX, int startY, Span<TerrainTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= oldChunkWidth - 1)
                return;

            int oldX = startX + deltaSize;
            int tileX = newSize;

            Span<TerrainTile> oldChunk = stackalloc TerrainTile[oldSize * oldSize];

            for (int y = 0; y < deltaSize; y++)
            {
                int oldY = startY + y;
                int tileY = y << oldSizeShift;

                LoadOldChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + (tileY + k) * (newSize + 1)] = oldChunk[k * oldSize];
                }
            }
        }

        private unsafe void LoadBottomDelimiters(int startX, int startY, Span<TerrainTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startY >= oldChunkHeight - 1)
                return;

            int oldY = startY + deltaSize;
            int tileY = newSize;

            Span<TerrainTile> oldChunk = stackalloc TerrainTile[oldSize * oldSize];

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;
                int tileX = x << oldSizeShift;

                LoadOldChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + k + tileY * (newSize + 1)] = oldChunk[k];
                }
            }
        }

        private unsafe void LoadCornerDelimiter(int startX, int startY, Span<TerrainTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= oldChunkWidth - 1 && startY >= oldChunkHeight - 1)
                return;

            int oldX = startX + deltaSize;
            int oldY = startY + deltaSize;

            int tileX = newSize;
            int tileY = newSize;

            Span<TerrainTile> oldChunk = stackalloc TerrainTile[oldSize * oldSize];

            LoadOldChunk(oldX, oldY, oldChunk);

            tiles[tileX + tileY * (newSize + 1)] = oldChunk[0];
        }

        private void LoadOldChunk(int x, int y, Span<TerrainTile> tiles)
        {
            Debug.Assert(tiles.Length == oldSize * oldSize);

            if (x >= oldChunkWidth || y >= oldChunkHeight)
                return;

            map.FillChunk(x, y, tiles);

            for (int i = 0; i < tiles.Length; i++)
            {
                ref TerrainTile tile = ref tiles[i];
                tile.Id = TerrainTileTranscoder.GetNewId(tile.Id);
            }
        }

        public void Dispose()
        {
            map.Dispose();
        }
    }
}
