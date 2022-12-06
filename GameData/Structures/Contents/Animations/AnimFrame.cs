using System.Runtime.InteropServices;

namespace GameData.Structures.Contents.Animations
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AnimFrame
    {
        public AnimFrameHeader Header;
        public byte[] Data;
    }
}
