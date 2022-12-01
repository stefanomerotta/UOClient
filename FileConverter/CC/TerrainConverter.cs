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
        private const int newSize = 64;
        private const int newSizeShift = 6;
        private const int deltaSizeShift = newSizeShift - oldSizeShift;
        private const int deltaSize = newSize >> deltaSizeShift;

        private readonly int width;
        private readonly int height;
        private readonly int oldChunkWidth;
        private readonly int oldChunkHeight;
        private readonly int newChunkWidth;
        private readonly int newChunkHeight;
        private readonly string path;
        private readonly UOPMap map;

        public TerrainConverter(string path, int id, int width, int height)
        {
            this.path = path;
            this.width = width;
            this.height = height;

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

            Span<MapTile> newChunk = MemoryMarshal.Cast<NewMapTile, MapTile>(chunk);

            LoadOldChunks(oldX, oldY, newChunk);
            LoadRightDelimiters(oldX, oldY, newChunk);
            LoadBottomDelimiters(oldX, oldY, newChunk);
            LoadCornerDelimiter(oldX, oldY, newChunk);
        }

        private unsafe void LoadOldChunks(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            Span<MapTile> oldChunk = stackalloc MapTile[oldSize * oldSize];

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;
                int tileX = x << deltaSizeShift;

                for (int y = 0; y < deltaSize; y++)
                {
                    int oldY = startY + y;
                    int tileY = y << deltaSizeShift;

                    LoadOldChunk(oldX, oldY, oldChunk);

                    for (int k = 0; k < oldSize; k++)
                    {
                        oldChunk.Slice(k * oldSize, oldSize).CopyTo(tiles.Slice(tileX + (tileY + k) * (newSize + 1), oldSize));
                    }
                }
            }
        }

        private unsafe void LoadRightDelimiters(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= oldChunkWidth - 1)
                return;

            int oldX = startX + deltaSize;
            const int tileX = newSize;

            Span<MapTile> oldChunk = stackalloc MapTile[oldSize * oldSize];

            for (int y = 0; y < deltaSize; y++)
            {
                int oldY = startY + y;
                int tileY = y << deltaSizeShift;

                LoadOldChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + (tileY + k) * (newSize + 1)] = oldChunk[k * oldSize];
                }
            }
        }

        private unsafe void LoadBottomDelimiters(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startY >= oldChunkHeight - 1)
                return;

            int oldY = startY + deltaSize;
            const int tileY = newSize;

            Span<MapTile> oldChunk = stackalloc MapTile[oldSize * oldSize];

            for (int x = 0; x < deltaSize; x++)
            {
                int oldX = startX + x;
                int tileX = x << deltaSizeShift;

                LoadOldChunk(oldX, oldY, oldChunk);

                for (int k = 0; k < oldSize; k++)
                {
                    tiles[tileX + k + tileY * (newSize + 1)] = oldChunk[k];
                }
            }
        }

        private unsafe void LoadCornerDelimiter(int startX, int startY, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == (newSize + 1) * (newSize + 1));

            if (startX >= oldChunkWidth - 1 && startY >= oldChunkHeight - 1)
                return;

            int oldX = startX + deltaSize;
            int oldY = startY + deltaSize;

            const int tileX = newSize;
            const int tileY = newSize;

            Span<MapTile> oldChunk = stackalloc MapTile[oldSize * oldSize];

            LoadOldChunk(oldX, oldY, oldChunk);

            tiles[tileX + tileY * (newSize + 1)] = oldChunk[0];
        }

        private void LoadOldChunk(int x, int y, Span<MapTile> tiles)
        {
            Debug.Assert(tiles.Length == oldSize * oldSize);

            if (x >= oldChunkWidth || y >= oldChunkHeight)
                return;

            map.FillChunk(x, y, tiles);

            for (int i = 0; i < tiles.Length; i++)
            {
                ref MapTile tile = ref tiles[i];
                tile.Id = TerrainTileTranscoder.GetNewId(tile.Id);
            }
        }

        public void Dispose()
        {
            map.Dispose();
        }
    }
}
