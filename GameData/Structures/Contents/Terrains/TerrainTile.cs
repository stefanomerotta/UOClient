using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Terrains
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct TerrainTile
    {
        public readonly ushort Id;
        public readonly sbyte Z;

        public TerrainTile(ushort id, sbyte z)
        {
            Id = id;
            Z = z;
        }

        public override string ToString()
        {
            return $"{{Id:{Id} Z:{Z}}}";
        }
    }
}
