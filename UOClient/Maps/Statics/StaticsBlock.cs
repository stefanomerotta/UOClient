using GameData.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TexturePacker;
using UOClient.Data;
using UOClient.Effects.Vertices;
using UOClient.Maps.Components;

namespace UOClient.Maps.Statics
{
    internal sealed class StaticsBlock : IDisposable
    {
        private const int vertexSize = StaticsFile.BlockSize;
        private const int vertexLength = StaticsFile.BlockLength;

        private static readonly float ecRateo = (float)(1 / Math.Sqrt(64 * 64 / 2));
        private static readonly float ccRateo = (float)(1 / Math.Sqrt(44 * 44 / 2));

        public readonly StaticTile[][] Tiles;
        private readonly TextureFile textureFile;
        private readonly Packer packer;

        private StaticsVertex[] vertices;
        private short[] indices;
        private TextureBounds[] textureBounds;

        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;

        public ushort X;
        public ushort Y;

        public int TotalStaticsCount { get; private set; }
        public Texture2D Texture { get; private set; }
        public int TextureWidth { get; private set; }
        public int TextureHeight { get; private set; }

        public StaticsBlock(TextureFile textureFile)
        {
            this.textureFile = textureFile;

            Tiles = new StaticTile[vertexLength][];
            packer = new(4096, 4096);
        }

        public void Initialize(StaticData[] staticsData, int totalStaticsCount)
        {
            //totalStaticsCount = 3;

            TotalStaticsCount = totalStaticsCount;
            if (totalStaticsCount == 0)
                return;

            vertices = new StaticsVertex[totalStaticsCount * 4];
            indices = new short[totalStaticsCount * 6];

            int startX = X << TerrainFile.BlockSizeShift;
            int startY = Y << TerrainFile.BlockSizeShift;

            int vIndex = 0;
            int iIndex = 0;

            Dictionary<ushort, Rectangle> addedTextures = new();
            int validTextures = 0;
            int maxTextureWidth = 0;
            int maxTextureHeight = 0;

            //Test(staticsData, addedTextures);

            for (int y = 0; y < vertexSize; y++)
            {
                for (int x = 0; x < vertexSize; x++)
                {
                    int index = x + y * vertexSize;

                    StaticTile[] tiles = Tiles[index];
                    if (tiles.Length == 0)
                        continue;

                    for (int k = 0; k < tiles.Length; k++)
                    {
                        StaticTile tile = tiles[k];
                        ref StaticData data = ref staticsData[tile.Id];

                        if (data.TextureId is >= ushort.MaxValue || data.Type != StaticTileType.Static)
                            continue;

                        //// TEMP
                        //if (data.TextureId is not 6549 and not 6553 and not 6554)
                        //    continue;

                        ref Rectangle rect = ref CollectionsMarshal.GetValueRefOrAddDefault
                        (
                            addedTextures,
                            (ushort)data.TextureId,
                            out bool exists
                        );

                        if (!exists)
                        {
                            textureFile.GetTextureSize(data.TextureId, out ushort width, out ushort height);
                            PackedRectangle packed = packer.Pack(width, height);

                            rect = Unsafe.As<PackedRectangle, Rectangle>(ref packed);
                            if (rect == default)
                                continue;

                            maxTextureWidth = Math.Max(maxTextureWidth, rect.Right);
                            maxTextureHeight = Math.Max(maxTextureHeight, rect.Bottom);

                            validTextures++;
                        }
                        else if (rect == default)
                            continue;

                        BuildBillboard(startX + x, startY + y, tile.Z, in data, vIndex, iIndex, in rect);

                        vIndex += 4;
                        iIndex += 6;
                    }
                }
            }

            if (validTextures == 0)
            {
                TotalStaticsCount = 0;
                return;
            }

            TextureWidth = maxTextureWidth;
            TextureHeight = maxTextureHeight;
            textureBounds = new TextureBounds[validTextures];

            int i = 0;
            foreach (ushort textureId in addedTextures.Keys)
            {
                ref Rectangle rect = ref CollectionsMarshal.GetValueRefOrNullRef(addedTextures, textureId);
                if (rect == default)
                    continue;

                textureBounds[i++] = new(textureId, in rect);
            }
        }

        public void SendToVRAM(GraphicsDevice device)
        {
            if (TotalStaticsCount == 0)
                return;

            vBuffer = new(device, StaticsVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            iBuffer = new(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            Texture = new(device, TextureWidth, TextureHeight, false, SurfaceFormat.Dxt5);

            vBuffer.SetData(vertices);
            iBuffer.SetData(indices);

            byte[] buffer = new byte[1024 * 1024];

            for (int i = 0; i < textureBounds.Length; i++)
            {
                ref TextureBounds entry = ref textureBounds[i];

                int count = textureFile.FillTexture(entry.TextureId, buffer);
                Texture.SetData(0, entry.Bounds, buffer, 0, count);
            }
        }

        public void Draw(GraphicsDevice device)
        {
            if (TotalStaticsCount == 0)
                return;

            device.SetVertexBuffer(vBuffer);
            device.Indices = iBuffer;

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TotalStaticsCount * 2);
        }

        public void ClearVRAM()
        {
            if (TotalStaticsCount == 0)
                return;

            vBuffer.Dispose();
            iBuffer.Dispose();
            Texture.Dispose();
            vBuffer = null;
            iBuffer = null;
            Texture = null;
        }

        public void Dispose()
        {
            ClearVRAM();
        }

        private void BuildBillboard(int x, int y, int z, in StaticData data, int vIndex, int iIndex, in Rectangle rect)
        {
            float rateo = data.Enhanced ? ecRateo : ccRateo;

            Vector3 position = new(x, z, y);
            short index = (short)vIndex;

            float startX = data.OffsetX * rateo;
            float startY = data.OffsetY * IsometricCamera.HeightRateo * rateo;
            float endX = (data.OffsetX + data.EndX - data.StartX) * rateo;
            float endY = (data.OffsetY + data.EndY - data.StartY) * IsometricCamera.HeightRateo * rateo;

            float textureStartX = rect.X + data.StartX;
            float textureStartY = rect.Y + data.EndY;
            float textureEndX = rect.X + data.EndX;
            float textureEndY = rect.Y + data.StartY;

            float depthOffset = 0;

            if (data.Flags.Has(StaticFlags.Background))
                depthOffset = -0.001f;

            else if (data.Flags.Has(StaticFlags.Roof))
                depthOffset = 0.002f;

            else if (data.Flags.Has(StaticFlags.Foliage))
                depthOffset = 10;

            Vector4 lowerLeft = new(startX, startY, textureStartX, textureStartY);
            Vector4 lowerRight = new(endX, startY, textureEndX, textureStartY);
            Vector4 upperLeft = new(startX, endY, textureStartX, textureEndY);
            Vector4 upperRight = new(endX, endY, textureEndX, textureEndY);

            vertices[vIndex++] = new(position, lowerLeft, depthOffset);
            vertices[vIndex++] = new(position, upperLeft, depthOffset);
            vertices[vIndex++] = new(position, upperRight, depthOffset);
            vertices[vIndex] = new(position, lowerRight, depthOffset);

            indices[iIndex++] = index;
            indices[iIndex++] = (short)(index + 1);
            indices[iIndex++] = (short)(index + 2);
            indices[iIndex++] = (short)(index + 2);
            indices[iIndex++] = (short)(index + 3);
            indices[iIndex] = index;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct TextureBounds
        {
            public readonly ushort TextureId;
            public readonly Rectangle Bounds;

            public TextureBounds(ushort textureId, in Rectangle bounds)
            {
                TextureId = textureId;
                Bounds = bounds;
            }
        }
    }
}
