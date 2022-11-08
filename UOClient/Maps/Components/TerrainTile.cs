using System.Runtime.InteropServices;

namespace UOClient.Maps.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TerrainTile
    {
        public ushort Id;
        public sbyte Z;
    }
}
