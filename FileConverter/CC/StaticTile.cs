using System.Runtime.InteropServices;

namespace FileConverter.CC
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct StaticTile
    {
        public ushort Id;
        public byte X;
        public byte Y;
        public sbyte Z;
        public ushort Color;
    }
}
