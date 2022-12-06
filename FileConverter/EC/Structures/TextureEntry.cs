using GameData.Enums;
using System.Runtime.InteropServices;

namespace TileDataExporter.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureEntry
    {
        public int DictionaryIndex;
        public float TextureStretch;
        public StaticTileType Type;
    }
}
