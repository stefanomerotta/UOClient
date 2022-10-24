using System.Runtime.InteropServices;

namespace TexturePacker.Components
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int W;
        public int H;
        public int X;
        public int Y;
        public int WasPacked;
    }
}
