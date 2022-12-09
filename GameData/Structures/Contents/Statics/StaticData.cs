using GameData.Enums;
using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Statics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct StaticData
    {
        public readonly ushort Id;
        public readonly StaticTextureInfo ECTexture;
        public readonly StaticTextureInfo CCTexture;
        public readonly RadarColor RadarColor;
        public readonly StaticTileType Type;
        public readonly StaticFlags Flags;
        public readonly StaticProperties Properties;

        public StaticData(ushort id)
        {
            Id = id;
        }
    }
}
