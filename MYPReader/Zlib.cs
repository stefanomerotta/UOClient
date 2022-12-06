using MYPReader.Enums;
using System.Runtime.InteropServices;

namespace MYPReader
{
    public static class Zlib
    {
        [DllImport("Zlib32", EntryPoint = "uncompress")]
        private static extern ZLibError Uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);

        [DllImport("Zlib64", EntryPoint = "uncompress")]
        private static extern ZLibError Uncompress64(byte[] dest, ref int destLen, byte[] source, int sourceLen);

        public static ZLibError Unzip(byte[] dest, byte[] source)
        {
            int destLength = dest.Length;
            return Unzip(dest, ref destLength, source, source.Length);
        }

        public static ZLibError Unzip(byte[] dest, ref int destLength, byte[] source, int sourceLength)
        {
            return Environment.Is64BitProcess
                ? Uncompress64(dest, ref destLength, source, sourceLength)
                : Uncompress(dest, ref destLength, source, sourceLength);
        }
    }
}