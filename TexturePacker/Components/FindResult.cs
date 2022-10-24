using System.Runtime.InteropServices;

namespace TexturePacker.Components
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct FindResult
    {
        public int X;
        public int Y;
        public Node** PrevLink;
    }
}
