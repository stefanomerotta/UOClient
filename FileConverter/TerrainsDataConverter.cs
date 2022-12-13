using Common.Utilities;
using FileConverter.EC;
using FileConverter.Utilities;
using FileSystem.Enums;
using FileSystem.IO;
using GameData.Enums;
using GameData.Structures.Contents.Terrains;
using GameData.Structures.Headers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FileConverter
{
    internal sealed class TerrainsDataConverter : IDisposable
    {
        private readonly TerrainDefinitionEntry[] definitions;
        private readonly TextureLoader textureLoader;
        private readonly string outPath;

        public TerrainsDataConverter(string uoECPath, string outPath)
        {
            textureLoader = new(Path.Combine(uoECPath, "Texture.uop"), "build/worldart/");

            this.outPath = outPath;

            TerrainDefinitionEntry[] entries = JsonReader.Read<TerrainDefinitionEntry[]>(".\\Content\\TerrainDefinition.json");
            
            definitions = new TerrainDefinitionEntry[entries.Max(entry => entry.Id) + 1];

            foreach (TerrainDefinitionEntry entry in entries)
            {
                Debug.Assert(definitions[entry.Id] == default);

                definitions[entry.Id] = entry;
            }
        }

        public void Convert(string dataFileName, string textureFileName)
        {
            HashSet<int> textureIds = new();

            TerrainDefinitionConverter.ConvertDefinitions(out List<SolidTerrainData> solid, 
                out List<LiquidTerrainData> liquid, out List<SingleTerrainData> single);

            using FileStream stream = File.Create(Path.Combine(outPath, dataFileName));
            PackageWriter writer = new(stream);

            WriteSolidTerrainData(solid, textureIds, ref writer);
            WriteLiquidTerrainData(liquid, textureIds, ref writer);
            WriteSingleTerrainData(single, textureIds, ref writer);

            writer.Dispose();

            WriteTextures(textureIds, textureFileName);
        }

        private void WriteSolidTerrainData(List<SolidTerrainData> solid, HashSet<int> textureIds, ref PackageWriter writer)
        {
            SolidTerrainData[] solidData = new SolidTerrainData[solid.Count];
            
            Span<SolidTerrainData> solidSpan = CollectionsMarshal.AsSpan(solid);

            for (int i = 0; i < solid.Count; i++)
            {
                ref SolidTerrainData data = ref solidSpan[i];
                ref TerrainDefinitionEntry def = ref definitions[data.Id];

                solidData[i] = new(data.Id, def.Type, data.Texture0, data.Texture1, data.AlphaMask);

                textureIds.Add(data.Texture0.Id);
                textureIds.Add(data.Texture1.Id);
                textureIds.Add(data.AlphaMask.Id);
            }

            writer.WriteSpan(0, solidData.AsReadOnlySpan(), CompressionAlgorithm.Zstd);
        }

        private void WriteLiquidTerrainData(List<LiquidTerrainData> liquid, HashSet<int> textureIds, ref PackageWriter writer)
        {
            LiquidTerrainData[] liquidData = new LiquidTerrainData[liquid.Count];
            Span<LiquidTerrainData> liquidSpan = CollectionsMarshal.AsSpan(liquid);

            for (int i = 0; i < liquid.Count; i++)
            {
                ref LiquidTerrainData data = ref liquidSpan[i];
                ref TerrainDefinitionEntry def = ref definitions[data.Id];

                liquidData[i] = new(data.Id, def.Type, def.Speed, def.WaveHeight, data.Normal, data.Texture0, data.Static);

                textureIds.Add(data.Normal.Id);
                textureIds.Add(data.Texture0.Id);
                textureIds.Add(data.Static.Id);
            }

            writer.WriteSpan(1, liquidData.AsReadOnlySpan(), CompressionAlgorithm.Zstd);
        }

        private void WriteSingleTerrainData(List<SingleTerrainData> single, HashSet<int> textureIds, ref PackageWriter writer)
        {
            SingleTerrainData[] singleData = new SingleTerrainData[single.Count];
            Span<SingleTerrainData> singleSpan = CollectionsMarshal.AsSpan(single);

            for (int i = 0; i < single.Count; i++)
            {
                ref SingleTerrainData data = ref singleSpan[i];
                ref TerrainDefinitionEntry def = ref definitions[data.Id];

                singleData[i] = new(data.Id, def.Type, data.TextureId);

                textureIds.Add(data.TextureId);
            }

            writer.WriteSpan(2, singleData.AsReadOnlySpan(), CompressionAlgorithm.Zstd);
        }

        private void WriteTextures(HashSet<int> textureIds, string fileName)
        {
            using FileStream stream = File.Create(Path.Combine(outPath, fileName));
            using PackageWriter<TerrainTextureMetadata> writer = new(stream);

            foreach(int id in textureIds.Order())
            {
                if (!textureLoader.TryLoad(id, out ReadOnlySpan<byte> data, out DDSFormat format, out int width, out int height))
                    continue;

                writer.WriteSpan(id, data, CompressionAlgorithm.Zstd, new((ushort)width, (ushort)height, (byte)format));
            }
        }

        public void Dispose()
        {
            textureLoader.Dispose();
        }

        private record struct TerrainDefinitionEntry
        {
            public ushort Id;
            public TerrainTileType Type;
            public float Speed;
            public float WaveHeight;
        }
    }
}
