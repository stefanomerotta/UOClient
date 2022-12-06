using System.Runtime.InteropServices;

namespace FileConverter.CC.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MapTile
    {
        public ushort Id;
        public sbyte Z;
    }
}
