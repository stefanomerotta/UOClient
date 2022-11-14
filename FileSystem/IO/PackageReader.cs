namespace FileSystem.IO
{
    public sealed class PackageReader : PackageReader<byte>
    {
        public PackageReader(FileStream fileStream)
            : base(fileStream)
        { }
    }
}
