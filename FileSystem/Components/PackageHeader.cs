using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileSystem.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PackageHeader
    {
        public const int HeaderAddressOffset = sizeof(byte) /* Version */ + sizeof(int) /* FileCount */;
        public static readonly int Size = Unsafe.SizeOf<PackageHeader>();

        public byte Version;
        public int FileCount;
        public int FirstHeaderAddress;
    }
}
