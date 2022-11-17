using TexturePacker.Enums;

namespace TexturePacker.Components
{
    internal unsafe struct Context : IDisposable
    {
        public int Width;
        public int Height;
        public int Align;
        public HeuristicSkylineType Heuristic;
        public int NumNodes;
        public Node* ActiveHead;
        public Node* FreeHead;
        public Node* Extra;
        public Node* AllNodes;

        public Context(int nodesCount, HeuristicSkylineType heuristicType)
        {
            if (nodesCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(nodesCount));

            Heuristic = heuristicType;

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
