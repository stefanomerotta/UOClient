using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace UOClient.Effects.Vertices
{
    public struct StaticsVertex : IVertexType, IEquatable<StaticsVertex>
    {
        public static readonly VertexDeclaration VertexDeclaration = new
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1)
        );

        public Vector3 Position;
        public Vector2 Bounds;
        public Vector3 Texture0;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public StaticsVertex(Vector3 position, Vector2 bounds, Vector3 texture0)
        {
            Position = position;
            Bounds = bounds;
            Texture0 = texture0;
        }

        public bool Equals(StaticsVertex other)
        {
            return Position == other.Position && Bounds == other.Bounds && Texture0 == other.Texture0;
        }

        public override bool Equals(object? obj)
        {
            return obj is StaticsVertex vertex && Equals(vertex);
        }

        public static bool operator ==(StaticsVertex left, StaticsVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StaticsVertex left, StaticsVertex right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine
            (
                Position.GetHashCode(),
                Bounds.GetHashCode(),
                Texture0.GetHashCode()
            );
        }
    }
}
