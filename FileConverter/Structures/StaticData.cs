using System.Runtime.InteropServices;

namespace FileConverter.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct StaticData
    {
        public int Id;
        public StaticTextureInfo ECTexture;
        public StaticTextureInfo CCTexture;
        public RadarColor RadarColor;
    }
}
