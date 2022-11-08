using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Effects.Vertices;
using UOClient.Maps.Components;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;
using VertexBuffer = Microsoft.Xna.Framework.Graphics.VertexBuffer;

namespace UOClient.Maps.Terrain
{
    internal sealed class TerrainBlock : IDisposable
    {
        private const int blockSize = MapFile.BlockSize;
        private const int terrainBlockLength = MapFile.TerrainBlockLength;
        private const int vertexSize = blockSize + 1;

        private readonly VertexPositionTextureIndex[] vertices;
        private readonly BitMapBlock64[] indices;
        private readonly VertexBuffer vBuffer;
        private readonly IndexBuffer?[] iBuffers;
        public readonly TerrainTile[] Tiles;

        public TerrainBlock(GraphicsDevice device)
        {
            Tiles = new TerrainTile[terrainBlockLength];
            vertices = new VertexPositionTextureIndex[terrainBlockLength];
            indices = new BitMapBlock64[(int)LandTileId.Length];
            vBuffer = new(device, VertexPositionTextureIndex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            iBuffers = new IndexBuffer[indices.Length];
        }

        public void Initialize(GraphicsDevice device, int blockX, int blockY)
        {
            SetUpVertices(blockX, blockY);
            SetUpIndices(device);
        }

        public void CleanUp()
        {
            for (int i = 0; i < iBuffers.Length; i++)
            {
                indices[i] = new();

                ref IndexBuffer? iBuffer = ref iBuffers[i];

                if (iBuffer is null)
                    continue;

                iBuffer.Dispose();
                iBuffer = null;
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

        public void Dispose()
        {
            vBuffer.Dispose();

            for (int i = 0; i < iBuffers.Length; i++)
                iBuffers[i]?.Dispose();
        }

        private void SetUpVertices(int blockX, int blockY)
        {
            Matrix m = Matrix.CreateTranslation(blockX * blockSize, 0, blockY * blockSize);

            for (int y = 0; y < vertexSize; y++)
            {
                for (int x = 0; x < vertexSize; x++)
                {
                    int index = x + y * vertexSize;

                    ref VertexPositionTextureIndex vertex = ref vertices[index];
                    TerrainTile tile = Tiles[index];

                    vertex.Position = Vector3.Transform(new Vector3(x, tile.Z, y), m);
                    vertex.TextureIndex = tile.Id;
                }
            }

            vBuffer.SetData(vertices);
        }

        private void SetUpIndices(GraphicsDevice device)
        {
            short[] indices = new short[blockSize * blockSize * 6];

            for (int i = 0; i < (int)LandTileId.Length; i++)
            {
                int counter = 0;
                ref BitMapBlock64 flags = ref SetupBitMap(i);

                for (int y = 0; y < blockSize; y++)
                {
                    int yVertexSize = y * vertexSize;

                    for (int x = 0; x < blockSize; x++)
                    {
                        if (!flags[x, y])
                            continue;

                        short topLeft = (short)(x + yVertexSize);
                        short topRight = (short)(topLeft + 1);
                        short lowerLeft = (short)(topLeft + vertexSize);
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

                ref IndexBuffer? iBuffer = ref iBuffers[i];

                iBuffer = new(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
                iBuffer.SetData(indices, 0, counter);

                Array.Clear(indices, 0, counter);
            }
        }

        private ref BitMapBlock64 SetupBitMap(int id)
        {
            ref BitMapBlock64 indices = ref this.indices[id];

            for (int y = 0; y < blockSize; y++)
            {
                int ySize = y * vertexSize;

                for (int x = 0; x < blockSize; x++)
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
                x = Math.Clamp(x, 0, blockSize - 1);
                y = Math.Clamp(y, 0, blockSize - 1);

                indices[x, y] = true;
            }
        }
    }
}
