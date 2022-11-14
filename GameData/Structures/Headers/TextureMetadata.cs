namespace GameData.Structures.Headers
{
    public readonly struct TextureMetadata
    {
        public readonly ushort Width;
        public readonly ushort Height;

        public TextureMetadata(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }
    }
}
