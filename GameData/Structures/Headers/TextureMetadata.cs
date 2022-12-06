using System.Runtime.InteropServices;

namespace GameData.Structures.Headers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct TextureMetadata
    {
        public readonly ushort Width;
        public readonly ushort Height;

        public TextureMetadata(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }
    }
}
