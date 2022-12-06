using Microsoft.Xna.Framework;

namespace UOClient.ECS.Components
{
    internal readonly struct CameraPosition
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public CameraPosition(Vector3 target)
        {
            X = (int)target.X;
            Y = (int)target.Y;
            Z = (int)target.Z;
        }
    }
}
