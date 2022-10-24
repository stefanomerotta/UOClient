namespace TexturePacker.Components
{
    internal unsafe struct Context : IDisposable
    {
        public int Width;
        public int Height;
        public int Align;
        public int InitMode;
        public int Heuristic;
        public int NumNodes;
        public Node* ActiveHead;
        public Node* FreeHead;
        public Node* Extra;
        public Node* AllNodes;

        public Context(int nodesCount)
        {
            if (nodesCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(nodesCount));

            Width = Height = Align = InitMode = Heuristic = NumNodes = 0;
            ActiveHead = FreeHead = null;

            // Allocate nodes
            AllNodes = (Node*)CRuntime.Malloc(sizeof(Node) * nodesCount);

            // Allocate extras
            Extra = (Node*)CRuntime.Malloc(sizeof(Node) * 2);
        }

        public void Dispose()
        {
            if (AllNodes is not null)
            {
                CRuntime.Free(AllNodes);
                AllNodes = null;
            }

            if (Extra is not null)
            {
                CRuntime.Free(Extra);
                Extra = null;
            }
        }
    }
}
