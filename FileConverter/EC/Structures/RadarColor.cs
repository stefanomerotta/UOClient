using System.Runtime.InteropServices;

namespace TileDataExporter.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RadarColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }
}
