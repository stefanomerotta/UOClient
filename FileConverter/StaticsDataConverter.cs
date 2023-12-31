﻿using Common.Utilities;
using FileConverter.EC;
using FileSystem.Enums;
using FileSystem.IO;
using GameData.Structures.Contents.Statics;
using GameData.Structures.Headers;

namespace FileConverter
{
    internal sealed class StaticsDataConverter : IDisposable
    {
        private static readonly TileDataComparer tileDataComparer = new();

        private readonly TileDataConverter converter;
        private readonly TextureLoader ecTextureLoader;
        private readonly TextureLoader ccTextureLoader;
        private readonly byte[] unusedCCTextureData;
        private readonly string outPath;

        public StaticsDataConverter(string ecPath, string outPath, StringDictionary dictionary)
        {
            this.outPath = outPath;

            converter = new(ecPath, dictionary);
            ecTextureLoader = new(Path.Combine(ecPath, "Texture.uop"), "build/worldart/");
            ccTextureLoader = new(Path.Combine(ecPath, "LegacyTexture.uop"), "build/tileartlegacy/");

            if (!ccTextureLoader.TryLoad(506, out ReadOnlySpan<byte> unused, out _, out _))
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

        private void ConvertStaticsData(List<StaticData> data, string fileName)
        {
            using FileStream stream = File.Create(Path.Combine(outPath, fileName));
            using PackageWriter writer = new(stream);

            writer.WriteSpan(0, data.Where(d => d.Id >= 0).ToArray().AsReadOnlySpan(), CompressionAlgorithm.Zstd);
        }

        private HashSet<MissingIdData> ConvertTextures(List<StaticData> data, string ecFileName, string ccFileName)
        {
            using FileStream ecStream = File.Create(Path.Combine(outPath, ecFileName));
            using PackageWriter<StaticTextureMetadata> ecWriter = new(ecStream);

            using FileStream ccStream = File.Create(Path.Combine(outPath, ccFileName));
            using PackageWriter<StaticTextureMetadata> ccWriter = new(ccStream);

            HashSet<int> ecIds = new();
            HashSet<int> ccIds = new();

            HashSet<MissingIdData> missings = new();

            for (int i = 0; i < data.Count; i++)
            {
                StaticData @static = data[i];

                int ecId = @static.ECTexture.Id;
                int ccId = @static.CCTexture.Id;

                ReadOnlySpan<byte> ecTexture = Span<byte>.Empty;
                int ecWidth = 0;
                int ecHeight = 0;

                bool hasECTexture = ecId < ushort.MaxValue && ecTextureLoader.TryLoad(ecId, out ecTexture, out ecWidth, out ecHeight);

                bool hasCCTexture = ccTextureLoader.TryLoad(ccId, out ReadOnlySpan<byte> ccTexture, out int ccWidth, out int ccHeight);

                if (hasCCTexture)
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
                        data[i] = new(ushort.MaxValue);
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

        private void WriteMissingIds(HashSet<MissingIdData> data)
        {
            using FileStream stream = File.Create(Path.Combine(outPath, "missings.txt"));
            using StreamWriter writer = new(stream);

            MissingIdData[] missings = data.OrderBy(x => x.Id).ToArray();

            for (int i = 0; i < missings.Length; i++)
            {
                ref MissingIdData entry = ref missings[i];

                writer.WriteLine($"{entry.Id};{entry.ECTextureId};{entry.CCTextureId};{entry.MissingEC};{entry.MissingCC}");
            }
        }

        public void Dispose()
        {
            converter.Dispose();
            ccTextureLoader.Dispose();
            ecTextureLoader.Dispose();
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
