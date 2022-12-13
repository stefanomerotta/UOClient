using FileSystem.IO;
using GameData.Structures.Contents.Terrains;
using System;
using System.IO;
using System.Linq;
using UOClient.Maps.Components;

namespace UOClient.Data
{
    internal sealed class TerrainsDataFile : IDisposable
    {
        private readonly PackageReader reader;

        public TerrainsDataFile()
        {
            FileStream stream = File.OpenRead(Path.Combine(Settings.FilePath, "terraindata.bin"));
            reader = new(stream);
        }

        public TerrainData Load()
        {
            SolidTerrainData[] solidData = reader.ReadArray<SolidTerrainData>(0);
            LiquidTerrainData[] liquidData = reader.ReadArray<LiquidTerrainData>(1);
            SingleTerrainData[] singleData = reader.ReadArray<SingleTerrainData>(2);

            SolidTerrainData[] solids = new SolidTerrainData[solidData.Max(data => data.Id) + 1];
            LiquidTerrainData[] liquids = new LiquidTerrainData[liquidData.Max(data => data.Id) + 1];
            SingleTerrainData[] singles = new SingleTerrainData[singleData.Max(data => data.Id) + 1];

            for (int i = 0; i < solidData.Length; i++)
            {
                ref SolidTerrainData entry = ref solidData[i];
                solids[entry.Id] = entry;
            }

            for (int i = 0; i < liquidData.Length; i++)
            {
                ref LiquidTerrainData entry = ref liquidData[i];
                liquids[entry.Id] = entry;
            }

            for (int i = 0; i < singleData.Length; i++)
            {
                ref SingleTerrainData entry = ref singleData[i];
                singles[entry.Id] = entry;
            }

            return new(liquids, solids, singles);
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
