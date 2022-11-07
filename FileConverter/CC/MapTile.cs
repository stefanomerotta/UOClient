using System.Runtime.InteropServices;

namespace FileConverter.CC
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MapTile
    {
        public ushort Id;
        public sbyte Z;
    }
}
