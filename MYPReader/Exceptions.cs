using MYPReader.Enums;

namespace MYPReader
{
    public sealed class InvalidCompressionException : SystemException
    {
        public InvalidCompressionException(CompressionFlag flag)
            : base($"Invalid compression flag: {flag}!")
        { }
    }

    public sealed class CompressionException : SystemException
    {
        public CompressionException(ZLibError error)
            : base(string.Format("Error compressing/decompressing: {0}", error))
        { }
    }
}
