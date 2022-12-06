namespace MYPReader.Enums
{
    /// <summary>
    /// Type of compression.
    /// </summary>
    public enum CompressionFlag : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Zlib.
        /// </summary>
        Zlib = 0x1
    }
}
