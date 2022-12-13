using FileSystem.IO;
using System;
using System.IO;
using UOClient.Maps.Components;
using FS = GameData.Structures.Contents.Statics;

namespace UOClient.Data
{
    internal sealed class StaticsDataFile : IDisposable
    {
        private readonly PackageReader reader;

        public StaticsDataFile()
        {
            FileStream stream = File.OpenRead(Path.Combine(Settings.FilePath, "staticdata.bin"));
            reader = new(stream);
        }

        public unsafe StaticData[] Load(bool legacy)
        {
            Span<FS.StaticData> buffer = reader.ReadSpan<FS.StaticData>(0);
            StaticData[] toRet = new StaticData[ushort.MaxValue];

            for (int i = 0; i < buffer.Length; i++)
            {
                ref readonly FS.StaticData data = ref buffer[i];
                ref readonly FS.StaticTextureInfo texture = ref data.ECTexture;

                bool usedLegacyTexture = false;

                if (legacy || texture.Id < 0)
                {
                    texture = ref data.CCTexture;
                    usedLegacyTexture = true;
                }

                if (data.Id == ushort.MaxValue)
                    continue;

                toRet[data.Id] = new(texture.Id, texture.StartX, texture.StartY, texture.EndX, texture.EndY,
                    texture.OffsetX, texture.OffsetY, data.Type, data.Flags, !usedLegacyTexture, data.Properties.Height);
            }

            return toRet;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
