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
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public Vector3 Position;
        public Vector3 Texture0;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public StaticsVertex(Vector3 position, Vector3 texture0)
        {
            Position = position;
            Texture0 = texture0;
        }

        public bool Equals(StaticsVertex other)
        {
            return Position == other.Position && Texture0 == other.Texture0;
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
                Texture0.GetHashCode()
            );
        }
    }
}
