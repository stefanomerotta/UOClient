using Microsoft.Xna.Framework;

namespace UOClient.ECS.Events
{
    internal readonly struct PositionChanged
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public PositionChanged(Vector3 target)
        {
            X = (int)target.X;
            Y = (int)target.Y;
            Z = (int)target.Z;
        }
    }
}
