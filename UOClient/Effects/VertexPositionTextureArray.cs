using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace UOClient.Effects
{
    public struct VertexPositionTextureArray : IVertexType, IEquatable<VertexPositionTextureArray>
    {
        public static readonly VertexDeclaration VertexDeclaration = new
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public Vector3 Position;
        public Vector3 TextureCoordinate;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionTextureArray(Vector3 position, Vector3 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position.GetHashCode(), TextureCoordinate.GetHashCode());
        }

        public override string ToString()
        {
            return $"{{Position:{Position} TextureCoordinate:{TextureCoordinate}}}";
        }

        public bool Equals(VertexPositionTextureArray other)
        {
            return Position == other.Position && TextureCoordinate == other.TextureCoordinate;
        }

        public override bool Equals(object? obj)
        {
            return obj is VertexPositionTextureArray texture && Equals(texture);
        }

        public static bool operator ==(VertexPositionTextureArray left, VertexPositionTextureArray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionTextureArray left, VertexPositionTextureArray right)
        {
            return !(left == right);
        }
    }
}