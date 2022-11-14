using GameData.Enums;
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
        private const int blockSize = TerrainFile.BlockSize;
        private const int terrainBlockLength = TerrainFile.BlockLength;
        private const int vertexSize = blockSize + 1;

        private readonly TerrainVertex[] vertices;
        private readonly short[][] indices;
        private readonly IndexBuffer?[] iBuffers;
        public readonly TerrainTile[] Tiles;
        private VertexBuffer vBuffer = null!;

        public ushort X;
        public ushort Y;

        public TerrainBlock()
        {
            Tiles = new TerrainTile[terrainBlockLength];
            vertices = new TerrainVertex[terrainBlockLength];
            indices = new short[(int)LandTileId.Length][];
            iBuffers = new IndexBuffer[indices.Length];
        }

        public void Initialize()
        {
            SetUpIndices();
        }

        public void SendToVRAM(GraphicsDevice device)
        {
            UpdateVertexBuffer(device, X, Y);
            UpdateIndexBuffers(device);
        }

        public void ClearVRAM()
        {
            for (int i = 0; i < iBuffers.Length; i++)
            {
                indices[i] = null!;

                ref IndexBuffer? iBuffer = ref iBuffers[i];

                if (iBuffer is null)
                    continue;

                iBuffer.Dispose();
                iBuffer = null;
            }
        }

        public void Draw(GraphicsDevice device, int id)
        {
            short[] indices = this.indices[id];

            if (indices.Length == 0)
                return;

            device.SetVertexBuffer(vBuffer);
            device.Indices = iBuffers[id];

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Length);
        }

        public void Dispose()
        {
            vBuffer.Dispose();

            for (int i = 0; i < iBuffers.Length; i++)
            {
                iBuffers[i]?.Dispose();
                iBuffers[i] = null;
            }
        }

        private void UpdateVertexBuffer(GraphicsDevice device, int blockX, int blockY)
        {
            vBuffer = new(device, TerrainVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);

            int startX = blockX << TerrainFile.BlockSizeShift;
            int startY = blockY << TerrainFile.BlockSizeShift;

            for (int y = 0; y < vertexSize; y++)
            {
                for (int x = 0; x < vertexSize; x++)
                {
                    int index = x + y * vertexSize;

                    ref TerrainVertex vertex = ref vertices[index];
                    TerrainTile tile = Tiles[index];

                    vertex.Position = new(startX + x, tile.Z, startY + y);
                    vertex.TextureIndex = tile.Id;
                }
            }

            vBuffer.SetData(vertices);
        }

        private void UpdateIndexBuffers(GraphicsDevice device)
        {
            for (int i = 0; i < (int)LandTileId.Length; i++)
            {
                short[] indices = this.indices[i];
                int indicesCount = indices.Length;

                if (indicesCount == 0)
                    continue;

                IndexBuffer iBuffer = new(device, IndexElementSize.SixteenBits, indicesCount, BufferUsage.WriteOnly);
                iBuffer.SetData(indices, 0, indicesCount);

                iBuffers[i] = iBuffer;
            }
        }

        private void SetUpIndices()
        {
            for (int i = 0; i < (int)LandTileId.Length; i++)
            {
                int counter = 0;
                BitMapBlock64 bitmap = new();
                SetupBitMap(i, ref bitmap);

                if (bitmap.TrueCount == 0)
                {
                    this.indices[i] = Array.Empty<short>();
                    continue;
                }

                short[] indices = new short[bitmap.TrueCount * 6];

                for (int y = 0; y < blockSize; y++)
                {
                    int yVertexSize = y * vertexSize;

                    for (int x = 0; x < blockSize; x++)
                    {
                        if (!bitmap[x, y])
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

                this.indices[i] = indices;
            }
        }

        private void SetupBitMap(int id, ref BitMapBlock64 bitmap)
        {
            for (int y = 0; y < blockSize; y++)
            {
                int ySize = y * vertexSize;

                for (int x = 0; x < blockSize; x++)
                {
                    if (Tiles[x + ySize].Id != id)
                        continue;

                    SetBit(ref bitmap, x, y);
                    SetBit(ref bitmap, x - 1, y);
                    SetBit(ref bitmap, x, y - 1);
                    SetBit(ref bitmap, x - 1, y - 1);
                    SetBit(ref bitmap, x, y + 1);
                    SetBit(ref bitmap, x + 1, y);
                    SetBit(ref bitmap, x + 1, y + 1);
                }
            }

            static void SetBit(ref BitMapBlock64 bitmap, int x, int y)
            {
                x = Math.Clamp(x, 0, blockSize - 1);
                y = Math.Clamp(y, 0, blockSize - 1);

                bitmap[x, y] = true;
            }
        }
    }
}
