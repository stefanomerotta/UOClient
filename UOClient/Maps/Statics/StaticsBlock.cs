using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TexturePacker;
using UOClient.Data;
using UOClient.Effects.Vertices;
using UOClient.Maps.Components;
using UOClient.Utilities;

namespace UOClient.Maps.Statics
{
    internal sealed class StaticsBlock : IDisposable
    {
        private const int vertexSize = StaticsFile.BlockSize;
        private const int vertexLength = StaticsFile.BlockLength;

        public readonly StaticTile[][] Tiles;
        private readonly TextureBuffer textureBuffer;
        private StaticsVertex[] vertices;
        private short[] indices;
        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;

        public ushort X;
        public ushort Y;

        public int TotalStaticsCount { get; private set; }
        public TextureArray TextureArray { get; private set; }

        public StaticsBlock(TextureFile textureFile)
        {
            Tiles = new StaticTile[vertexLength][];
            textureBuffer = new(textureFile);
        }

        public void Initialize(StaticData[] staticsData, int totalStaticsCount)
        {
            if (totalStaticsCount == 0)
                return;

            TotalStaticsCount = totalStaticsCount;

            vertices = new StaticsVertex[totalStaticsCount];
            indices = new short[totalStaticsCount * 6];

            int startX = X << TerrainFile.BlockSizeShift;
            int startY = Y << TerrainFile.BlockSizeShift;

            int vIndex = 0;
            int iIndex = 0;

            for (int y = 0; y < vertexSize; y++)
            {
                for (int x = 0; x < vertexSize; x++)
                {
                    int index = x + y * vertexSize;

                    StaticTile[] tiles = Tiles[index];
                    if (tiles.Length == 0)
                        continue;

                    for (int z = 0; z < tiles.Length; z++)
                    {
                        StaticTile tile = tiles[z];
                        ref StaticData data = ref staticsData[tile.Id];

                        if (data.TextureId is >= ushort.MaxValue)
                            continue;

                        //PackedRectangle rect = textureBuffer.GetOrAdd((ushort)data.TextureId);
                        //BuildBillboard(startX + x, startY + y, tile.Z, ref data, vIndex, iIndex, 0);

                        vIndex += 4;
                        iIndex += 6;
                    }
                }
            }
        }

        public void SendToVRAM(GraphicsDevice device)
        {
            if (TotalStaticsCount == 0)
                return;

            vBuffer = new(device, StaticsVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            iBuffer = new(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);

            vBuffer.SetData(vertices);
            iBuffer.SetData(indices);
        }

        public void Draw(GraphicsDevice device)
        {
            if (TotalStaticsCount == 0)
                return;

            device.SetVertexBuffer(vBuffer);
            device.Indices = iBuffer;

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TotalStaticsCount);
        }

        public void ClearVRAM()
        {
            if (TotalStaticsCount == 0)
                return;

            vBuffer.Dispose();
            iBuffer.Dispose();
            vBuffer = null;
            iBuffer = null;

            TextureArray.Dispose();
            TextureArray = null;

            textureBuffer.Reset();
        }

        public void Dispose()
        {
            ClearVRAM();
        }

        private void BuildBillboard(int x, int y, int z, ref StaticData data, int vIndex, int iIndex, in PackedRectangle rect)
        {
            float rateo = (float)(1 / Math.Sqrt(64 * 64 / 2));
            Vector3 position = new(x + 0.5f, z, y + 1.5f);
            short index = (short)vIndex;

            float startX = data.OffsetX * rateo;
            float startY = -data.OffsetY * 10 * rateo;
            float endX = (data.EndX - data.StartX) * rateo;
            float endY = (data.EndY - data.StartY) * 10 * rateo;

            float textureStartX = data.StartX + rect.X;
            float textureStartY = data.StartY + rect.Y;
            float textureEndX = data.EndX + rect.X;
            float textureEndY = data.EndY + rect.Y;

            Vector4 lowerLeft = new(startX, startY, textureStartX, textureEndY);
            Vector4 lowerRight = new(endX, startY, textureEndX, textureEndY);
            Vector4 upperLeft = new(startX, endY, textureStartX, textureStartY);
            Vector4 upperRight = new(endX, endY, textureEndX, textureEndY);

            vertices[vIndex++] = new(position, lowerLeft);
            vertices[vIndex++] = new(position, upperLeft);
            vertices[vIndex++] = new(position, upperRight);
            vertices[vIndex] = new(position, lowerRight);

            indices[iIndex++] = index;
            indices[iIndex++] = (short)(index + 1);
            indices[iIndex++] = (short)(index + 2);
            indices[iIndex++] = (short)(index + 2);
            indices[iIndex++] = (short)(index + 3);
            indices[iIndex] = index;
        }
    }
}
