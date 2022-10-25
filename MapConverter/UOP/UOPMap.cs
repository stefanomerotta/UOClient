﻿using System.Runtime.InteropServices;
using MapConverter.IO;

namespace MapConverter.UOP
{
    internal class UOPMap
    {
        private const int chunkSize = 196;

        private readonly FileReader reader;
        private readonly long[] chunks;
        private readonly int chunksWidth;
        private readonly int chunksHeight;

        public UOPMap(string filePath, int id, int width, int height)
        {
            chunksWidth = width >> 3;
            chunksHeight = height >> 3;

            reader = new(Path.Combine(filePath, $"map{id}LegacyMUL.uop"));
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
                    chunks[index++] = uopChunk.Address + j * chunkSize;
                }
            }
        }

        public void FillChunk(int x, int y, Span<MapTile> chunk)
        {
            long address = chunks[x * chunksHeight + y];
            reader.Seek(address);

            reader.Skip(4);
            reader.ReadSpan(chunk);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MapTile
    {
        public ushort Id;
        public sbyte Z;
    }
}