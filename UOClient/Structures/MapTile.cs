using System.Runtime.InteropServices;

namespace UOClient.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct MapTile
    {
        public ushort Id;
        public sbyte Z;
    }
}
