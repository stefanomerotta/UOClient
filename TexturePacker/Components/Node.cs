using System.Runtime.InteropServices;

namespace TexturePacker.Components
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct Node
    {
        public int X;
        public int Y;
        public Node* Next;
    }
}
