using GameData.Enums;
using GameData.Structures.Contents.Terrains;
using System.Linq;

namespace UOClient.Maps.Components
{
    internal readonly struct TerrainData
    {
        public readonly int MaxLength;
        public readonly LiquidTerrainData[] Liquid;
        public readonly SolidTerrainData[] Solid;
        public readonly SingleTerrainData[] Single;
        public readonly TerrainTileType[] Types;

        public TerrainData(LiquidTerrainData[] liquid, SolidTerrainData[] solid, SingleTerrainData[] single)
        {
            Liquid = liquid;
            Solid = solid;
            Single = single;

            MaxLength = liquid.Select(data => data.Id)
                .Concat(solid.Select(data => data.Id))
                .Concat(single.Select(data => data.Id))
                .Max() + 1;

            Types = new TerrainTileType[MaxLength];

            for (int i = 0; i < liquid.Length; i++)
            {
                ref LiquidTerrainData data = ref liquid[i];
                if (data.Id == 0)
                    continue;

                Types[data.Id] = TerrainTileType.Liquid;
            }

            for (int i = 0; i < solid.Length; i++)
            {
                ref SolidTerrainData data = ref solid[i];
                if (data.Id == 0)
                    continue;

                Types[data.Id] = TerrainTileType.Solid;
            }

            for (int i = 0; i < single.Length; i++)
            {
                ref SingleTerrainData data = ref single[i];
                if (data.Id == 0)
                    continue;

                Types[data.Id] = TerrainTileType.Single;
            }
        }
    }
}
