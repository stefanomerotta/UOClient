using GameData.Enums;
using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Terrains
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SingleTerrainData
    {
        public readonly ushort Id;
        public readonly TerrainTileType Type;
        public readonly int TextureId;

        public SingleTerrainData(ushort id, TerrainTileType type, int textureId)
        {
            Id = id;
            Type = type;
            TextureId = textureId;
        }

        public override string ToString()
        {
            return $"{{ Id:{Id} TextureId:{TextureId} Type:{Type} }}";
        }
    }
}
