namespace MYPReader.Enums
{
    public enum ZLibError
    {
        Okay = 0,
        StreamEnd = 1,
        NeedDictionary = 2,
        FileError = -1,
        StreamError = -2,
        DataError = -3,
        MemoryError = -4,
        BufferError = -5,
        VersionError = -6,
    }
}
