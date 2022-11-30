using Common.Utilities;
using FileConverter.Structures;
using FileSystem.Enums;
using FileSystem.IO;
using GameData.Structures.Headers;

namespace FileConverter
{
    internal class StaticsDataConverter
    {
        private static readonly TileDataComparer tileDataComparer = new();

        private readonly EC.TileDataConverter converter;
        private readonly EC.TextureLoader ecTextureLoader;
        private readonly EC.TextureLoader ccTextureLoader;
        private readonly byte[] unusedCCTextureData;

        public StaticsDataConverter(string path)
        {
            converter = new(path);
            ecTextureLoader = new(Path.Combine(path, "Texture.uop"), "build/worldart/");
            ccTextureLoader = new(Path.Combine(path, "LegacyTexture.uop"), "build/tileartlegacy/");

            if (!ccTextureLoader.TryLoad(506, out Span<byte> unused, out _, out _))
                throw new Exception("Failed to load unused texture for compare");

            unusedCCTextureData = unused.ToArray();
        }

        public void Convert(string staticsDataName, string ecTextureName, string ccTextureName)
        {
            List<StaticData> converted = converter.ConvertTileData();
            converted.Sort(tileDataComparer);

            HashSet<MissingIdData> missings = ConvertTextures(converted, ecTextureName, ccTextureName);
            ConvertStaticsData(converted, staticsDataName);
            WriteMissingIds(missings);
        }

        private static void ConvertStaticsData(List<StaticData> data, string fileName)
        {
            using FileStream stream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", fileName));
            using PackageWriter writer = new(stream);

            writer.WriteSpan(0, data.Where(d => d.Id >= 0).ToArray().AsReadOnlySpan(), CompressionAlgorithm.Zstd);
        }

        private HashSet<MissingIdData> ConvertTextures(List<StaticData> data, string ecFileName, string ccFileName)
        {
            using FileStream ecStream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", ecFileName));
            using PackageWriter<TextureMetadata> ecWriter = new(ecStream);

            using FileStream ccStream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", ccFileName));
            using PackageWriter<TextureMetadata> ccWriter = new(ccStream);

            HashSet<int> ecIds = new();
            HashSet<int> ccIds = new();

            HashSet<MissingIdData> missings = new();

            for (int i = 0; i < data.Count; i++)
            {
                StaticData @static = data[i];

                int ecId = @static.ECTexture.Id;
                int ccId = @static.CCTexture.Id;

                bool hasECTexture = ecTextureLoader.TryLoad(ecId, out Span<byte> ecTexture, out int ecWidth, out int ecHeight);
                bool hasCCTexture = ccTextureLoader.TryLoad(ccId, out Span<byte> ccTexture, out int ccWidth, out int ccHeight);

                if(hasCCTexture)
                    hasCCTexture = !unusedCCTextureData.AsSpan().SequenceEqual(ccTexture!);

                if (!hasECTexture || !hasCCTexture)
                {
                    missings.Add(new()
                    {
                        Id = @static.Id,
                        ECTextureId = ecId,
                        CCTextureId = ccId,
                        MissingEC = !hasECTexture,
                        MissingCC = !hasCCTexture
                    });

                    if (!hasECTexture && !hasCCTexture)
                    {
                        data[i] = new() { Id = ushort.MaxValue };
                        continue;
                    }
                }

                if (hasECTexture && ecIds.Add(ecId))
                    ecWriter.WriteSpan(ecId, ecTexture, CompressionAlgorithm.Zstd, new((ushort)ecWidth, (ushort)ecHeight));

                if (hasCCTexture && ccIds.Add(ccId))
                    ccWriter.WriteSpan(ccId, ccTexture, CompressionAlgorithm.Zstd, new((ushort)ccWidth, (ushort)ccHeight));
            }

            return missings;
        }

        private static void WriteMissingIds(HashSet<MissingIdData> data)
        {
            using FileStream stream = File.Create(Path.Combine("C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic\\", "missings.txt"));
            using StreamWriter writer = new(stream);

            MissingIdData[] missings = data.OrderBy(x => x.Id).ToArray();

            for (int i = 0; i < missings.Length; i++)
            {
                ref MissingIdData entry = ref missings[i];

                writer.WriteLine($"{entry.Id};{entry.ECTextureId};{entry.CCTextureId};{entry.MissingEC};{entry.MissingCC}");
            }
        }

        private class TileDataComparer : IComparer<StaticData>
        {
            public int Compare(StaticData x, StaticData y)
            {
                return x.Id.CompareTo(y.Id);
            }
        }

        private struct MissingIdData
        {
            public int Id;
            public int ECTextureId;
            public int CCTextureId;
            public bool MissingEC;
            public bool MissingCC;
        }
    }
}
