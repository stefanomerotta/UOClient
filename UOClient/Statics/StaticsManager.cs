using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using UOClient.Effects;
using UOClient.Effects.Vertices;

namespace UOClient.Statics
{
    internal class StaticsManager
    {
        private GraphicsDevice device;
        private IsometricCamera camera;
        private StaticsEffect effect;
        private Texture2D textureAtlas;

        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;

        public void Initialize(GraphicsDevice device, ContentManager contentManager, IsometricCamera camera)
        {
            this.device = device;
            this.camera = camera;

            byte[] temp = new byte[256 * 256];
            Texture2D t = new(device, 1024, 256, false, SurfaceFormat.Dxt5);

            //contentManager.Load<Texture2D>("statics/00000005").GetData(temp, 0, 128 * 128);
            //t.SetData(0, new Rectangle(0, 0, 128, 128), temp, 0, 128 * 128);


            //contentManager.Load<Texture2D>("statics/00000128").GetData(temp, 0, 128 * 256);
            //t.SetData(0, new Rectangle(0, 0, 128, 256), temp, 0, 128 * 256);

            //Array.Clear(temp);
            //contentManager.Load<Texture2D>("statics/00000131").GetData(temp, 0, 128 * 256);
            //t.SetData(0, new Rectangle(256, 0, 128, 256), temp, 0, 128 * 256);


            //contentManager.Load<Texture2D>("statics/00000302").GetData(temp, 0, 64 * 256);
            //t.SetData(0, new Rectangle(0, 0, 64, 256), temp, 0, 64 * 256);

            //Array.Clear(temp);
            //contentManager.Load<Texture2D>("statics/00000299").GetData(temp, 0, 128 * 256);
            //t.SetData(0, new Rectangle(256, 0, 128, 256), temp, 0, 128 * 256);

            //Array.Clear(temp);
            //contentManager.Load<Texture2D>("statics/00000303").GetData(temp, 0, 64 * 256);
            //t.SetData(0, new Rectangle(384, 0, 64, 256), temp, 0, 64 * 256);


            contentManager.Load<Texture2D>("statics/00000149").GetData(temp, 0, 64 * 256);
            t.SetData(0, new Rectangle(0, 0, 64, 256), temp, 0, 64 * 256);

            Array.Clear(temp);
            contentManager.Load<Texture2D>("statics/00000144").GetData(temp, 0, 128 * 256);
            t.SetData(0, new Rectangle(256, 0, 128, 256), temp, 0, 128 * 256);

            Array.Clear(temp);
            contentManager.Load<Texture2D>("statics/00000153").GetData(temp, 0, 64 * 256);
            t.SetData(0, new Rectangle(384, 0, 64, 256), temp, 0, 64 * 256);

            textureAtlas = t;

            effect = new(contentManager)
            {
                World = camera.WorldMatrix,
                View = camera.ViewMatrix,
                Projection = camera.ProjectionMatrix,
                Texture0 = textureAtlas,
                TextureSize = new(1024, 256),
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

            //StaticData first = new()
            //{
            //    TileHeight = 30,

            //    StartX = 0,
            //    StartY = 0,
            //    Width = 30,
            //    Height = 96,
            //    OffsetX = 6,
            //    //OffsetY = 0
            //};

            //StaticData second = new()
            //{
            //    TileHeight = 30,

            //    StartX = 23,
            //    StartY = 0,
            //    Width = 45,
            //    Height = 105,
            //    OffsetX = -4,
            //    OffsetY = -24
            //};



            //StaticData first = new()
            //{
            //    TileHeight = 30,

            //    StartX = 0,
            //    StartY = 0,
            //    Width = 65,
            //    Height = 194,
            //    OffsetX = 0,
            //    OffsetY = -2
            //};

            //StaticData second = new()
            //{
            //    TileHeight = 30,

            //    StartX = 256,
            //    StartY = 0,
            //    Width = 65,
            //    Height = 194,
            //    OffsetX = 1,
            //    OffsetY = -2
            //};



            //StaticData first = new()
            //{
            //    TileHeight = 30,

            //    StartX = 0,
            //    StartY = 0,
            //    Width = 48,
            //    Height = 170,
            //    OffsetX = -2,
            //    OffsetY = 0
            //};

            //StaticData second = new()
            //{
            //    TileHeight = 30,

            //    StartX = 256,
            //    StartY = 0,
            //    Width = 70,
            //    Height = 168,
            //    OffsetX = -2,
            //    OffsetY = 0
            //};

            //StaticData third = new()
            //{
            //    TileHeight = 30,

            //    StartX = 384,
            //    StartY = 0,
            //    Width = 48,
            //    Height = 168,
            //    OffsetX = 22,
            //    OffsetY = 0
            //};



            StaticData first = new()
            {
                StartX = 0,
                StartY = 0,
                Width = 55,
                Height = 171,
                OffsetX = -5,
                OffsetY = 0
            };

            StaticData second = new()
            {
                StartX = 256,
                StartY = 0,
                Width = 74,
                Height = 167,
                OffsetX = -5,
                OffsetY = -3
            };

            StaticData third = new()
            {
                StartX = 384,
                StartY = 0,
                Width = 55,
                Height = 171,
                OffsetX = 15,
                OffsetY = 0
            };

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

        private struct StaticData
        {
            public short StartX;
            public short StartY;
            public short Width;
            public short Height;

            public float OffsetX;
            public float OffsetY;

            public Color RadarColor;
        }
    }
}
