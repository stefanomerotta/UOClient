using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using UOClient.Effects.Vertices;
using UOClient.Structures;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;
using VertexBuffer = Microsoft.Xna.Framework.Graphics.VertexBuffer;

namespace UOClient.Terrain
{
    internal sealed class TerrainBlock : IDisposable
    {
        public const int Size = 64;
        public const int SizeOffset = 6;
        public const int VertexSize = Size + 1;

        private readonly int blockX;
        private readonly int blockY;

        private readonly TerrainTile[] tiles;
        private readonly VertexPositionTextureIndex[] vertices;
        private readonly VertexPositionColor[] boundaries;
        private readonly BitMapBlock64[] indices;

        private readonly VertexBuffer vBuffer;
        private readonly IndexBuffer[] iBuffers;

        public TerrainBlock(GraphicsDevice device, int blockX, int blockY, TerrainTile[] tiles)
        {
            Debug.Assert(tiles.Length == VertexSize * VertexSize);

            this.blockX = blockX;
            this.blockY = blockY;
            this.tiles = tiles;

            vertices = new VertexPositionTextureIndex[VertexSize * VertexSize];
            indices = new BitMapBlock64[(int)LandTileId.Length];
            boundaries = new VertexPositionColor[8];

            vBuffer = new(device, VertexPositionTextureIndex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            iBuffers = new IndexBuffer[indices.Length];

            SetUpVertices();
            SetUpIndices(device);

            SetUpBoundaries();
        }

        private void SetUpVertices()
        {
            Matrix m = Matrix.CreateTranslation(blockX * Size, 0, blockY * Size);

            for (int y = 0; y < VertexSize; y++)
            {
                for (int x = 0; x < VertexSize; x++)
                {
                    int index = x + y * VertexSize;

                    ref VertexPositionTextureIndex vertex = ref vertices[index];
                    TerrainTile tile = tiles[index];

                    vertex.Position = Vector3.Transform(new Vector3(x, tile.Z, y), m);
                    vertex.TextureIndex = tile.Id;
                }
            }

            vBuffer.SetData(vertices);
        }

        private ref BitMapBlock64 SetupBitMap(int id)
        {
            ref BitMapBlock64 indices = ref this.indices[id];

            for (int y = 0; y < Size; y++)
            {
                int ySize = y * VertexSize;

                for (int x = 0; x < Size; x++)
                {
                    if (vertices[x + ySize].TextureIndex != id)
                        continue;

                    SetBit(ref indices, x, y);
                    SetBit(ref indices, x - 1, y);
                    SetBit(ref indices, x, y - 1);
                    SetBit(ref indices, x - 1, y - 1);
                    SetBit(ref indices, x, y + 1);
                    SetBit(ref indices, x + 1, y);
                    SetBit(ref indices, x + 1, y + 1);
                }
            }

            return ref indices;

            static void SetBit(ref BitMapBlock64 indices, int x, int y)
            {
                x = Math.Clamp(x, 0, Size - 1);
                y = Math.Clamp(y, 0, Size - 1);

                indices[x, y] = true;
            }
        }

        private void SetUpIndices(GraphicsDevice device)
        {
            short[] indices = new short[Size * Size * 6];

            for (int i = 0; i < (int)LandTileId.Length; i++)
            {
                int counter = 0;
                ref BitMapBlock64 flags = ref SetupBitMap(i);

                for (int y = 0; y < Size; y++)
                {
                    int yVertexSize = y * VertexSize;

                    for (int x = 0; x < Size; x++)
                    {
                        if (!flags[x, y])
                            continue;

                        short topLeft = (short)(x + yVertexSize);
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

                if (counter == 0)
                    continue;

                iBuffers[i] = new(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                iBuffers[i].SetData(indices, 0, counter);

                Array.Clear(indices, 0, counter);
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

            void SetVertex(VertexPositionTextureIndex v, int i)
            {
                ref VertexPositionColor vertex = ref boundaries[i];
                vertex.Position = v.Position;
                vertex.Color = Color.White;
            }
        }

        public void Draw(GraphicsDevice device, int id)
        {
            ref BitMapBlock64 flags = ref indices[id];

            if (flags.TrueCount == 0)
                return;

            device.SetVertexBuffer(vBuffer);
            device.Indices = iBuffers[id];

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, flags.TrueCount * 4);
        }

        public void DrawBoundaries(GraphicsDevice device)
        {
            device.DrawUserPrimitives(PrimitiveType.LineList, boundaries, 0, 4, VertexPositionColor.VertexDeclaration);
        }

        public void Dispose()
        {
            vBuffer.Dispose();

            for (int i = 0; i < iBuffers.Length; i++)
                iBuffers[i]?.Dispose();
        }
    }
}
