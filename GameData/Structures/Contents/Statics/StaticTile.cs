using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Statics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct StaticTile
    {
        public readonly ushort Id;
        public readonly ushort Color;
        public readonly sbyte Z;

        public StaticTile(ushort id, ushort color, sbyte z)
        {
            Id = id;
            Color = color;
            Z = z;
        }

        public override string ToString()
        {
            return $"{{Id:{Id} Color:{Color} Z:{Z}}}";
        }
    }
}
