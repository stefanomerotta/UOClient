using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace FileConverter
{
    internal static class Extensions
    {
        public static T Read<T>(this BinaryReader reader) where T : struct
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
            reader.Read(buffer);

            return MemoryMarshal.Read<T>(buffer);
        }

        public static void Skip(this BinaryReader reader, int offset)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        public static bool SequenceEqual(this byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
                return false;

            for(int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }
    }
}
