using GameData.Enums;
using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Terrains
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SolidTerrainData
    {
        public readonly ushort Id;
        public readonly TerrainTileType Type;
        public readonly TerrainTextureInfo Texture0;
        public readonly TerrainTextureInfo Texture1;
        public readonly TerrainTextureInfo AlphaMask;

        public SolidTerrainData(ushort id, TerrainTileType type, TerrainTextureInfo texture0, 
            TerrainTextureInfo texture1, TerrainTextureInfo alphaMask)
        {
            Id = id;
            Type = type;
            Texture0 = texture0;
            Texture1 = texture1;
            AlphaMask = alphaMask;
        }
    }
}
