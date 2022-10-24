using FileSystem.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileSystem.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct FileHeader : IComparable<FileHeader>
    {
        public const int HeaderAddressOffset = sizeof(int); // Index
        public static readonly int Size = Unsafe.SizeOf<FileHeader>();

        public int Index;
        public int NextHeaderAddress;
        public CompressionAlgorithm CompressionAlgorithm;
        public int CompressedSize;
        public int UncompressedSize;
        public uint ContentHash;

        public int CompareTo(FileHeader other)
        {
            return Index.CompareTo(other.Index);
        }

        public static bool operator <(FileHeader left, FileHeader right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(FileHeader left, FileHeader right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(FileHeader left, FileHeader right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(FileHeader left, FileHeader right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
