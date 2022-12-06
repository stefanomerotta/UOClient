using System.Runtime.InteropServices;

namespace TileDataExporter.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureOffset
    {
        public int StartX;
        public int StartY;
        public int EndX;
        public int EndY;
        public int OffsetX;
        public int OffsetY;
    }
}
