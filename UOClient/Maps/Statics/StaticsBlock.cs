using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Effects.Vertices;
using UOClient.Maps.Components;

namespace UOClient.Maps.Statics
{
    internal sealed class StaticsBlock : IDisposable
    {
        private const int vertexSize = MapFile.BlockSize;
        private const int vertexLength = MapFile.StaticsBlockLength;

        public readonly StaticTile[][] Tiles;
        private StaticsVertex[] vertices;
        private short[] indices;
        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;

        public StaticsBlock()
        {
            Tiles = new StaticTile[vertexLength][];
        }

        public void Initialize(GraphicsDevice device, int blockX, int blockY, StaticData[] staticsData, int totalStaticsCount)
        {
            vertices = new StaticsVertex[totalStaticsCount];
            indices = new short[totalStaticsCount * 6];

            vBuffer = new(device, StaticsVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            iBuffer = new(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);

            int startX = blockX << MapFile.BlockSizeShift;
            int startY = blockY << MapFile.BlockSizeShift;

            int vIndex = 0;
            int iIndex = 0;

            for (int y = 0; y < vertexSize; y++)
            {
                for (int x = 0; x < vertexSize; x++)
                {
                    int index = x + y * vertexSize;
                    ref StaticTile[] tiles = ref Tiles[index];

                    for (int z = 0; z < tiles.Length; z++)
                    {
                        StaticTile tile = tiles[index];
                        ref StaticData data = ref staticsData[tile.Id];

                        BuildBillboard(startX + x, startY + y, tile.Z, ref data, vIndex, iIndex, 0);

                        vIndex += 4;
                        iIndex += 6;
                    }
                }
            }

            vBuffer.SetData(vertices);
            iBuffer.SetData(indices);
        }

        private void BuildBillboard(int x, int y, int z, ref StaticData data, int vIndex, int iIndex, int texureIndex)
        {
            float rateo = (float)(1 / Math.Sqrt(64 * 64 / 2));
            Vector3 position = new(x + 0.5f, z, y + 1.5f);
            short index = (short)vIndex;

            float width = data.EndX - data.StartX;
            float height = data.EndY - data.StartY;
            float bbWidth = width * rateo;
            float bbHeight = height * 10 * rateo;

            Vector2 lowerLeft = new(data.OffsetX * rateo, -data.OffsetY * 10 * rateo);
            Vector2 lowerRight = lowerLeft with { X = lowerLeft.X + bbWidth };
            Vector2 upperLeft = lowerLeft with { Y = lowerLeft.Y + bbHeight };
            Vector2 upperRight = lowerRight with { Y = lowerRight.Y + bbHeight };

            Vector3 textureLowerLeft = new(data.StartX, data.StartY + height, texureIndex);
            Vector3 textureLowerRight = new(data.StartX + width, data.StartY + height, texureIndex);
            Vector3 textureUpperLeft = new(data.StartX, data.StartY, texureIndex);
            Vector3 textureUpperRight = new(data.StartX + width, data.StartY, texureIndex);

            vertices[vIndex++] = new(position, lowerLeft, textureLowerLeft);
            vertices[vIndex++] = new(position, upperLeft, textureUpperLeft);
            vertices[vIndex++] = new(position, upperRight, textureUpperRight);
            vertices[vIndex] = new(position, lowerRight, textureLowerRight);

            indices[iIndex++] = index;
            indices[iIndex++] = (short)(index + 1);
            indices[iIndex++] = (short)(index + 2);
            indices[iIndex++] = (short)(index + 2);
            indices[iIndex++] = (short)(index + 3);
            indices[iIndex] = index;
        }

        public void CleanUp()
        {
            vBuffer.Dispose();
            iBuffer.Dispose();

            vBuffer = null;
            iBuffer = null;
        }

        public void Dispose()
        {
            vBuffer.Dispose();
            iBuffer.Dispose();

            vBuffer = null;
            iBuffer = null;
        }
    }
}
