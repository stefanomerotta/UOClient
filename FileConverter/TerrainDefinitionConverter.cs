using Common.Utilities;
using FileConverter.Utilities;
using GameData.Enums;
using GameData.Structures.Contents.Terrains;
using System.Runtime.InteropServices;

namespace FileConverter
{
    internal static class TerrainDefinitionConverter
    {
        public static void ConvertDefinitions(out List<SolidTerrainData> solid,
            out List<LiquidTerrainData> liquid, out List<SingleTerrainData> single)
        {
            JsonTerrainData[] entries = JsonReader.Read<JsonTerrainData[]>(".\\Content\\TerrainDefinition.json");

            solid = new(entries.Length);
            liquid = new(entries.Length);
            single = new(entries.Length);

            for (int i = 0; i < entries.Length; i++)
            {
                ref JsonTerrainData entry = ref entries[i];

                if (entry.Type.Has(TerrainTileType.Single))
                {
                    single.Add(new(entry.Id, entry.Type, entry.TextureId));
                }
                else if (entry.Type.Has(TerrainTileType.Liquid))
                {
                    ref readonly TerrainTextureInfo normal = ref UnsafeUtility.As<JsonTextureInfo, TerrainTextureInfo>(in entry.Normal);
                    ref readonly TerrainTextureInfo texture0 = ref UnsafeUtility.As<JsonTextureInfo, TerrainTextureInfo>(in entry.Texture0);
                    ref readonly TerrainTextureInfo @static = ref UnsafeUtility.As<JsonTextureInfo, TerrainTextureInfo>(in entry.Static);

                    liquid.Add(new(entry.Id, entry.Type, entry.Speed, entry.WaveHeight, normal, texture0, @static));
                }
                else
                {
                    ref readonly TerrainTextureInfo texture0 = ref UnsafeUtility.As<JsonTextureInfo, TerrainTextureInfo>(in entry.Texture0);
                    ref readonly TerrainTextureInfo texture1 = ref UnsafeUtility.As<JsonTextureInfo, TerrainTextureInfo>(in entry.Texture1);
                    ref readonly TerrainTextureInfo alphaMask = ref UnsafeUtility.As<JsonTextureInfo, TerrainTextureInfo>(in entry.AlphaMask);

                    solid.Add(new(entry.Id, entry.Type, texture0, texture1, alphaMask));
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct JsonTerrainData
        {
            public ushort Id;
            public float Speed;
            public float WaveHeight;
            public int TextureId;
            public TerrainTileType Type;
            public JsonTextureInfo Texture0;
            public JsonTextureInfo Texture1;
            public JsonTextureInfo AlphaMask;
            public JsonTextureInfo Normal;
            public JsonTextureInfo Static;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct JsonTextureInfo
        {
            public int Id;
            public float Stretch;
        }
    }
}
