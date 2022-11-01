using FileSystem.IO;
using System;
using System.IO;
using UOClient.IO;
using UOClient.Structures;
using UOClient.Terrain;

namespace UOClient.Data
{
    internal abstract class Map
    {
        public abstract void FillChunk(int chunkX, int chunkY, Span<MapTile> chunk);
    }

    internal class UOPMap : Map
    {
        private const int chunkSize = 196;

        private readonly FileReader reader;
        private readonly long[] chunks;
        private readonly int chunksWidth;
        private readonly int chunksHeight;

        public UOPMap(int id, int width, int height)
        {
            chunksWidth = width >> 3;
            chunksHeight = height >> 3;

            reader = new(Path.Combine(Settings.FilePath, $"map{id}LegacyMUL.uop"));
            UOPUnpacker unpacker = new(reader, $"build/map{id}legacymul/{{0:D8}}.dat");
            UOPFileContent[] uopChunks = unpacker.Unpack();

            chunks = new long[chunksWidth * chunksHeight];

            int index = 0;

            for (int i = 0; i < uopChunks.Length; i++)
            {
                UOPFileContent uopChunk = uopChunks[i];
                int chunksLength = uopChunk.Length / chunkSize;

                for (int j = 0; j < chunksLength && index < chunks.Length; j++)
                {
                    chunks[index++] = uopChunk.Address + (j * chunkSize);
                }
            }
        }

        public override void FillChunk(int x, int y, Span<MapTile> chunk)
        {
            long address = chunks[x + y * chunksWidth];
            reader.Seek(address);

            reader.Skip(4);
            reader.ReadSpan(chunk);
        }
    }

    internal sealed class MyMap : Map, IDisposable
    {
        private readonly PackageReader reader;
        private readonly int chunksHeight;

        public MyMap(int width, int height)
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, "converted.bin"), FileMode.Open);
            reader = new(stream);

            chunksHeight = (int)Math.Ceiling(height / (double)TerrainBlock.Size);
        }

        public override void FillChunk(int chunkX, int chunkY, Span<MapTile> chunk)
        {
            reader.ReadSpan(chunkX + chunkY * chunksHeight, chunk);
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
