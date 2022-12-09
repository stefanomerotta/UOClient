using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Statics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct RadarColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public RadarColor(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
