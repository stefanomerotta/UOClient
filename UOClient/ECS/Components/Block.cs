namespace UOClient.ECS.Components
{
    internal struct Block
    {
        public ushort X;
        public ushort Y;

        public Block(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }
    }
}
