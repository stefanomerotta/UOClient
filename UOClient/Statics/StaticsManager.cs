using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Data;
using UOClient.Effects;
using UOClient.Effects.Vertices;
using UOClient.Maps.Components;
using UOClient.Utilities;

namespace UOClient.Statics
{
    internal class StaticsManager
    {
        private readonly StaticData[] staticsData;

        private GraphicsDevice device;
        private IsometricCamera camera;
        private StaticsEffect effect;
        private TextureAtlas textureAtlas;

        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;

        public StaticsManager(bool legacy)
        {
            using StaticsDataFile staticsDataFile = new();
            staticsData = staticsDataFile.Load(legacy);
        }

        public void Initialize(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.device = device;
            this.camera = camera;

            textureAtlas = new(device, 4096, 4096);

            effect = new(contentManager)
            {
                World = camera.WorldMatrix,
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                Texture0 = textureAtlas,
                TextureSize = new(textureAtlas.Width, textureAtlas.Height),
                Rotation = Matrix.CreateRotationY(MathHelper.ToRadians(45))
            };

            vBuffer = new(device, StaticsVertex.VertexDeclaration, 12, BufferUsage.WriteOnly);
            iBuffer = new(device, IndexElementSize.SixteenBits, 18, BufferUsage.WriteOnly);
        }

        public void OnLocationChanged()
        {
            effect.View = camera.ViewMatrix;

            Vector3 target = new(185, 20, 300);
            StaticsVertex[] vertices = new StaticsVertex[12];

            BuildBillboard2(target, first, 0, vertices.AsSpan(0, 4));
            BuildBillboard2(target with { X = target.X + 1 }, second, 0, vertices.AsSpan(4, 4));
            BuildBillboard2(target with { X = target.X + 1, Z = target.Z - 1 }, third, 2, vertices.AsSpan(8, 4));

            vBuffer.SetData(vertices);

            short[] indices = new short[]
            {
                0, 1, 2,
                2, 3, 0,

                4, 5, 6,
                6, 7, 4,

                8, 9, 10,
                10, 11, 8
            };

            iBuffer.SetData(indices);
        }

        public void Draw()
        {
            device.RasterizerState = new()
            {
                CullMode = CullMode.None
            };

            var pass = effect.CurrentTechnique.Passes[0];
            pass.Apply();

            device.SetVertexBuffer(vBuffer);
            device.Indices = iBuffer;

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 6);
        }

        private void BuildBillboard(Vector3 position, StaticData data, int index, Span<StaticsVertex> vertices)
        {
            float rateo = 1 / 64f;

            position += new Vector3(1, 0, 1);

            float bbWidth = (data.Width / 2 + data.OffsetX) * rateo;
            float bbHeight = (float)((data.Height + data.OffsetY) * 10 * Math.Sqrt(2) * rateo);

            float bbX = data.OffsetX * rateo;
            float bbY = data.OffsetY * 10 * rateo;

            position += new Vector3(bbX, bbY, 0);

            Vector3 lowerLeft = position + new Vector3(-bbWidth, 0, bbWidth);
            Vector3 lowerRight = position + new Vector3(bbWidth, 0, -bbWidth);

            Vector3 upperLeft = lowerLeft with { Y = position.Y + bbHeight };
            Vector3 upperRight = lowerRight with { Y = position.Y + bbHeight };

            Vector3 textureLowerLeft = new(data.StartX, data.StartY + data.Height, index);
            Vector3 textureLowerRight = new(data.StartX + data.Width, data.StartY + data.Height, index);
            Vector3 textureUpperLeft = new(data.StartX, data.StartY, index);
            Vector3 textureUpperRight = new(data.StartX + data.Width, data.StartY, index);

            //vertices[0] = new(lowerLeft, textureLowerLeft);
            //vertices[1] = new(upperLeft, textureUpperLeft);
            //vertices[2] = new(upperRight, textureUpperRight);
            //vertices[3] = new(lowerRight, textureLowerRight);
        }

        private void BuildBillboard2(Vector3 position, StaticData data, int index, Span<StaticsVertex> vertices)
        {
            float rateo = (float)(1 / Math.Sqrt(64 * 64 / 2));
            float bbWidth = data.Width * rateo;
            float bbHeight = data.Height * 10 * rateo;

            //Matrix m = Matrix.CreateTranslation(new Vector3(data.OffsetX * rateo, -data.OffsetY * 10 * rateo, 0))
            //    * Matrix.CreateRotationY(MathHelper.ToRadians(45))
            //    * Matrix.CreateTranslation(position);
            Matrix m = Matrix.CreateTranslation(position);

            Vector2 lowerLeft = new(data.OffsetX * rateo, -data.OffsetY * 10 * rateo);
            Vector2 lowerRight = lowerLeft with { X = lowerLeft.X + bbWidth };
            Vector2 upperLeft = lowerLeft with { Y = lowerLeft.Y + bbHeight };
            Vector2 upperRight = lowerRight with { Y = lowerRight.Y + bbHeight };

            //Vector2 lowerLeft = Vector2.Zero;
            //Vector2 lowerRight = new(bbWidth, 0);
            //Vector2 upperLeft = lowerLeft with { Y = lowerLeft.Y + bbHeight };
            //Vector2 upperRight = lowerRight with { Y = lowerRight.Y + bbHeight };

            Vector3 textureLowerLeft = new(data.StartX, data.StartY + data.Height, index);
            Vector3 textureLowerRight = new(data.StartX + data.Width, data.StartY + data.Height, index);
            Vector3 textureUpperLeft = new(data.StartX, data.StartY, index);
            Vector3 textureUpperRight = new(data.StartX + data.Width, data.StartY, index);

            //lowerLeft = Vector3.Transform(lowerLeft, m);
            //lowerRight = Vector3.Transform(lowerRight, m);
            //upperLeft = Vector3.Transform(upperLeft, m);
            //upperRight = Vector3.Transform(upperRight, m);

            vertices[0] = new(position, lowerLeft, textureLowerLeft);
            vertices[1] = new(position, upperLeft, textureUpperLeft);
            vertices[2] = new(position, upperRight, textureUpperRight);
            vertices[3] = new(position, lowerRight, textureLowerRight);
        }
    }
}
