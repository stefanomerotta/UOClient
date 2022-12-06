using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace UOClient.Effects.Vertices
{
    public struct TerrainVertex : IVertexType, IEquatable<TerrainVertex>
    {
        public static readonly VertexDeclaration VertexDeclaration = new
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
        );

        public Vector3 Position;
        public ushort TextureIndex;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public TerrainVertex(Vector3 position, ushort textureIndex)
        {
            Position = position;
            TextureIndex = textureIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position.GetHashCode(), TextureIndex.GetHashCode());
        }

        public override string ToString()
        {
            return $"{{Position:{Position} TextureCoordinate:{TextureIndex}}}";
        }

        public bool Equals(TerrainVertex other)
        {
            return Position == other.Position && TextureIndex == other.TextureIndex;
        }

        public override bool Equals(object? obj)
        {
            return obj is TerrainVertex texture && Equals(texture);
        }

        public static bool operator ==(TerrainVertex left, TerrainVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TerrainVertex left, TerrainVertex right)
        {
            return !(left == right);
        }
    }
}