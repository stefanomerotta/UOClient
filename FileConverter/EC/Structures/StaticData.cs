using GameData.Enums;
using GameData.Structures.Contents.Statics;
using System.Runtime.InteropServices;

namespace FileConverter.EC.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct StaticData
    {
        public ushort Id;
        public StaticTextureInfo ECTexture;
        public StaticTextureInfo CCTexture;
        public RadarColor RadarColor;
        public StaticTileType Type;
        public StaticFlags Flags;
        public StaticProperties Properties;
    }
}
