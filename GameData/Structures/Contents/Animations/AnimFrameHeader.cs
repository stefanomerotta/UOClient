using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Animations
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AnimFrameHeader
    {
        public short CenterX;
        public short CenterY;
        public short Width;
        public short Height;
        public ushort DataLength;
    }
}
