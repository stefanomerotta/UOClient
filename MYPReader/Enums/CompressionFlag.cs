namespace MYPReader.Enums
{
    /// <summary>
    /// Type of compression.
    /// </summary>
    public enum CompressionFlag : short
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
