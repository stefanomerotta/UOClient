using FileSystem.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UOClient.Maps.Components;
using UOClient.Utilities;

namespace UOClient.Data
{
    internal sealed class MapFile : IDisposable
    {
        public const int BlockSize = 64;
        public const int BlockSizeShift = 6; // number of byteshift for converting between block and tile coordinates
        public const int TerrainBlockLength = (BlockSize + 1) * (BlockSize + 1);
        public const int StaticsBlockLength = BlockSize * BlockSize;

        private static readonly int terrainBlockByteLength = TerrainBlockLength * Unsafe.SizeOf<TerrainTile>();

        private readonly PackageReader reader;
        public readonly int BlocksWidth;
        public readonly int BlocksHeight;

        public MapFile(int width, int height)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, "converted.bin"), FileMode.Open);
            reader = new(stream);

            BlocksWidth = (int)Math.Ceiling(width / (double)BlockSize);
            BlocksHeight = (int)Math.Ceiling(height / (double)BlockSize);
        }

        public unsafe int FillBlock(int blockX, int blockY, TerrainTile[] terrain, StaticTile[][] statics)
        {
            Debug.Assert(blockX >= 0 && blockX < BlocksWidth);
            Debug.Assert(blockY >= 0 && blockY < BlocksHeight);
            Debug.Assert(terrain.Length == TerrainBlockLength);
            Debug.Assert(statics.Length == StaticsBlockLength);

            Span<byte> block = reader.ReadSpan(blockX + blockY * BlocksWidth);

            block[..terrainBlockByteLength].Cast<byte, TerrainTile>().CopyTo(terrain);

            Span<byte> staticsBlock = block[terrainBlockByteLength..];

            int counter = 0;
            int totalCount = 0;
            
            for (int i = 0; i < StaticsBlockLength; i++)
            {
                int count = staticsBlock[counter++];

                if (count == 0)
                {
                    statics[i] = Array.Empty<StaticTile>();
                    continue;
                }

                int byteCount = count * sizeof(StaticTile);

                statics[i] = staticsBlock.Slice(counter, byteCount).Cast<byte, StaticTile>().ToArray();
                totalCount += count;

                counter += byteCount;
            }

            return totalCount;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
