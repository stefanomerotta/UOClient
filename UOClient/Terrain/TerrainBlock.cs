using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;
using UOClient.Effects;
using UOClient.Structures;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;
using VertexDeclaration = Microsoft.Xna.Framework.Graphics.VertexDeclaration;
using VertexElement = Microsoft.Xna.Framework.Graphics.VertexElement;

namespace UOClient.Terrain
{
    internal class TerrainBlock
    {
        public const int Size = 64;
        public const int VertexSize = Size + 1;

        private readonly int blockX;
        private readonly int blockY;

        private readonly MapTile[,] tiles;
        private readonly VertexPositionTextureArray[] vertices;
        private readonly VertexPositionColor[] boundaries;
        private readonly short[] indices;

        public TerrainBlock(int blockX, int blockY, MapTile[,] tiles)
        {
            this.blockX = blockX;
            this.blockY = blockY;
            this.tiles = tiles;

            vertices = new VertexPositionTextureArray[VertexSize * VertexSize];
            indices = new short[(VertexSize - 1) * (VertexSize - 1) * 6];
            boundaries = new VertexPositionColor[8];

            SetUpVertices();
            SetUpIndices();
            SetUpBoundaries();
        }

        private void SetUpVertices()
        {
            Matrix m = Matrix.CreateTranslation(blockX * Size, 0, blockY * Size);

            for (int y = 0; y < VertexSize; y++)
            {
                for (int x = 0; x < VertexSize; x++)
                {
                    ref VertexPositionTextureArray vertex = ref vertices[x + y * VertexSize];
                    MapTile tile = tiles[x, y];

                    Vector3 position = Vector3.Transform(new Vector3(x, tile.Z, y), m);

                    vertex.Position = position;
                    vertex.TextureCoordinate.X = position.X / 10f;
                    vertex.TextureCoordinate.Y = position.Z / 10f;
                    vertex.TextureCoordinate.Z = tile.Id is > 2 and < 7 ? 0 : 1;

                    //vertex.Texture2Coordinate.X = position.X / 4f;
                    //vertex.Texture2Coordinate.Y = position.Z / 4f;
                    //vertex.Texture3Coordinate.X = position.X / 32f;
                    //vertex.Texture3Coordinate.Y = position.Z / 32f;
                }
            }
        }

        private void SetUpIndices()
        {
            int counter = 0;

            for (int y = 0; y < VertexSize - 1; y++)
            {
                for (int x = 0; x < VertexSize - 1; x++)
                {
                    short topLeft = (short)(x + y * VertexSize);
                    short topRight = (short)(topLeft + 1);
                    short lowerLeft = (short)(topLeft + VertexSize);
                    short lowerRight = (short)(lowerLeft + 1);

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = lowerLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        private void SetUpBoundaries()
        {
            SetVertex(vertices[0], 0);
            SetVertex(vertices[VertexSize - 1], 1);
            SetVertex(vertices[VertexSize - 1], 2);
            SetVertex(vertices[^1], 3);
            SetVertex(vertices[^1], 4);
            SetVertex(vertices[^VertexSize], 5);
            SetVertex(vertices[^VertexSize], 6);
            SetVertex(vertices[0], 7);

            void SetVertex(VertexPositionTextureArray v, int i)
            {
                ref VertexPositionColor vertex = ref boundaries[i];
                vertex.Position = v.Position;
                vertex.Color = Color.White;
            }
        }

        public void Draw(GraphicsDevice device)
        {
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0,
                indices.Length / 3, VertexPositionTextureArray.VertexDeclaration);
        }

        public void DrawBoundaries(GraphicsDevice device)
        {
            device.DrawUserPrimitives(PrimitiveType.LineList, boundaries, 0, 4, VertexPositionColor.VertexDeclaration);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VertexPositionDualTexture : IVertexType, IEquatable<VertexPositionDualTexture>
        {
            public static readonly VertexDeclaration VertexDeclaration;

            public Vector3 Position;
            public Vector2 TextureCoordinate;
            public Vector2 Texture2Coordinate;
            public Vector2 Texture3Coordinate;

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            public VertexPositionDualTexture(Vector3 position, Vector2 textureCoordinate, Vector2 texture2Coordinate, Vector2 texture3Coordinate)
            {
                Position = position;
                TextureCoordinate = textureCoordinate;
                Texture2Coordinate = texture2Coordinate;
                Texture3Coordinate = texture3Coordinate;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    Position.GetHashCode(),
                    TextureCoordinate.GetHashCode(),
                    Texture2Coordinate.GetHashCode(),
                    Texture3Coordinate.GetHashCode()
                );
            }

            public override string ToString()
            {
                return $"{{Position:{Position} TextureCoordinate: {TextureCoordinate} " +
                    $"Texture2Coordinate: {Texture2Coordinate} Texture3Coordinate: {Texture3Coordinate}}}";
            }

            bool IEquatable<VertexPositionDualTexture>.Equals(VertexPositionDualTexture other)
            {
                return Position == other.Position
                    && TextureCoordinate == other.TextureCoordinate
                    && Texture2Coordinate == other.Texture2Coordinate
                    && Texture3Coordinate == other.Texture3Coordinate;
            }

            public static bool operator ==(VertexPositionDualTexture left, VertexPositionDualTexture right)
            {
                return left.Position == right.Position
                    && left.TextureCoordinate == right.TextureCoordinate
                    && left.Texture2Coordinate == right.Texture2Coordinate
                    && left.Texture3Coordinate == right.Texture3Coordinate;
            }

            public static bool operator !=(VertexPositionDualTexture left, VertexPositionDualTexture right)
            {
                return !(left == right);
            }

            static VertexPositionDualTexture()
            {
                VertexDeclaration = new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                    new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2)
                );
            }

            public override bool Equals(object? obj)
            {
                return obj is VertexPositionDualTexture texture
                    && ((IEquatable<VertexPositionDualTexture>)this).Equals(texture);
            }
        }
    }
}
