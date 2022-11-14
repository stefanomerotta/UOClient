using FileSystem.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileSystem.Components
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct FileHeader<T> : IComparable<FileHeader<T>>
        where T : struct
    {
        public const int HeaderAddressOffset = sizeof(int); // Index
        public static readonly int Size = Unsafe.SizeOf<FileHeader<T>>();

        public int Index;
        public int NextHeaderAddress;
        public CompressionAlgorithm CompressionAlgorithm;
        public int CompressedSize;
        public int UncompressedSize;
        public uint ContentHash;
        public T Metadata;

        public int CompareTo(FileHeader<T> other)
        {
            return Index.CompareTo(other.Index);
        }

        public static bool operator <(FileHeader<T> left, FileHeader<T> right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(FileHeader<T> left, FileHeader<T> right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(FileHeader<T> left, FileHeader<T> right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(FileHeader<T> left, FileHeader<T> right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
