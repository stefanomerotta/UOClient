namespace UOClient.ECS.Components
{
    internal struct StaticTexture
    {
        public ushort TextureId;

        public StaticTexture(ushort textureId)
        {
            TextureId = textureId;
        }
    }
}
