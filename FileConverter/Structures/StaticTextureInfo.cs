using System.Runtime.InteropServices;

namespace FileConverter.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticTextureInfo
    {
        public int Id;
        public short StartX;
        public short StartY;
        public short EndX;
        public short EndY;
        public short OffsetX;
        public short OffsetY;
    }
}
