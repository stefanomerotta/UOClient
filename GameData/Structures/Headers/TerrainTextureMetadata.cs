using System.Runtime.InteropServices;

namespace GameData.Structures.Headers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct TerrainTextureMetadata
    {
        public readonly ushort Width;
        public readonly ushort Height;
        public readonly byte Format;

        public TerrainTextureMetadata(ushort width, ushort height, byte format)
        {
            Width = width;
            Height = height;
            Format = format;
        }
    }
}
