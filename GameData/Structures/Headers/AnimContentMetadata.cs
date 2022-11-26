using System.Runtime.InteropServices;

namespace GameData.Structures.Headers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct AnimContentMetadata
    {
        public readonly ushort AnimId;
        public readonly byte ActionId;
        public readonly byte Direction;
        public readonly byte FrameCount;

        public AnimContentMetadata(ushort animId, byte actionId, byte direction, byte frameCount)
        {
            AnimId = animId;
            ActionId = actionId;
            Direction = direction;
            FrameCount = frameCount;
        }
    }
}
