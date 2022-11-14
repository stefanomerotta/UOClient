namespace FileSystem.IO
{
    public sealed class PackageWriter : PackageWriter<byte>
    {
        public PackageWriter(FileStream fileStream)
            : base(fileStream)
        { }
    }
}
