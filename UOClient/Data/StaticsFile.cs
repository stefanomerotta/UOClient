using FileSystem.IO;
using System;
using System.Diagnostics;
using System.IO;
using UOClient.Maps.Components;
using UOClient.Utilities;

namespace UOClient.Data
{
    internal sealed class StaticsFile : IDisposable
    {
        public const int BlockSize = TerrainFile.BlockSize;
        public const int BlockSizeShift = TerrainFile.BlockSizeShift; // number of byteshift for converting between block and tile coordinates
        public const int BlockLength = BlockSize * BlockSize;

        private readonly PackageReader reader;
        public readonly int BlocksWidth;
        public readonly int BlocksHeight;

        public StaticsFile(int width, int height)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, "statics.bin"), FileMode.Open);
            reader = new(stream);

            BlocksWidth = (int)Math.Ceiling(width / (double)BlockSize);
            BlocksHeight = (int)Math.Ceiling(height / (double)BlockSize);
        }

        public unsafe int FillBlock(int blockX, int blockY, StaticTile[][] statics)
        {
            Debug.Assert(blockX >= 0 && blockX < BlocksWidth);
            Debug.Assert(blockY >= 0 && blockY < BlocksHeight);
            Debug.Assert(statics.Length == BlockLength);

            Span<byte> block = reader.ReadArray(blockX + blockY * BlocksWidth);

            if (block.Length == 0)
                return 0;

            int counter = 0;
            int totalCount = 0;

            for (int i = 0; i < BlockLength; i++)
            {
                int count = block[counter++];

                if (count == 0)
                {
                    statics[i] = Array.Empty<StaticTile>();
                    continue;
                }

                int byteCount = count * sizeof(StaticTile);

                statics[i] = block.Slice(counter, byteCount).Cast<byte, StaticTile>().ToArray();
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
