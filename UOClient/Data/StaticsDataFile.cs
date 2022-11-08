using FileSystem.IO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UOClient.Maps.Components;

namespace UOClient.Data
{
    internal sealed class StaticsDataFile : IDisposable
    {
        private readonly PackageReader reader;

        public StaticsDataFile()
        {
            FileStream stream = File.Open(Path.Combine(Settings.FilePath, "staticsData.bin"), FileMode.Open);
            reader = new(stream);
        }

        public unsafe StaticData[] Load(bool legacy)
        {
            Span<FileStaticData> buffer = reader.ReadSpan<FileStaticData>(0);
            StaticData[] toRet = new StaticData[ushort.MaxValue];

            for (int i = 0; i < buffer.Length; i++)
            {
                ref FileStaticData data = ref buffer[i];
                ref FileStaticTextureInfo texture = ref legacy ? ref data.CCTexture : ref data.ECTexture;

                toRet[data.Id] = new(texture.Id, texture.StartX, texture.StartY, texture.EndX, texture.EndY, texture.OffsetX, texture.OffsetY);
            }

            return toRet;
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FileStaticData
        {
            public ushort Id;
            public FileStaticTextureInfo ECTexture;
            public FileStaticTextureInfo CCTexture;
            public FileRadarColor RadarColor;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FileStaticTextureInfo
        {
            public int Id;
            public short StartX;
            public short StartY;
            public short EndX;
            public short EndY;
            public short OffsetX;
            public short OffsetY;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FileRadarColor
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }
    }
}
