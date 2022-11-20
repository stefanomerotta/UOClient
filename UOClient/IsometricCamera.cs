using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using UOClient.Utilities.Polyfills;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace UOClient
{
    public sealed class IsometricCamera
    {
        private const int maxHeight = 128;

        private static readonly Vector3 positionFromOrigin = new Vector3(1, (float)Math.Sqrt(2), 1) * maxHeight;
        
        private static readonly Vector3 left = new(-1, 0, 1);
        private static readonly Vector3 right = new(1, 0, -1);
        private static readonly Vector3 up = new(-1, 0, -1);
        private static readonly Vector3 down = new(1, 0, 1);

        private Vector3 target;
        private readonly Matrix worldMatrix;
        private readonly Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Matrix scaleMatrix;
        private Matrix worldViewMatrix;
        private Matrix worldViewProjectionMatrix;
        private Matrix invertedProjectionMatrix;

        public ref readonly Matrix WorldMatrix => ref worldMatrix;
        public ref readonly Matrix ViewMatrix => ref viewMatrix;
        public ref readonly Matrix ProjectionMatrix => ref projectionMatrix;
        public ref readonly Matrix ScaleMatrix => ref scaleMatrix;
        public ref readonly Matrix WorldViewMatrix => ref worldViewMatrix;
        public ref readonly Matrix WorldViewProjectionMatrix => ref worldViewProjectionMatrix;

        public Vector3 Target => target;
        public float Zoom { get; private set; }

        public IsometricCamera()
        {
            Zoom = 1;

            target = new(712, 0, 1367); //new(835, 0, 904);
            scaleMatrix = Matrix.CreateScale(Zoom, Zoom, 1);

            worldMatrix = Matrix.CreateScale(1, .1f, 1);

            projectionMatrix = Matrix.CreateTranslation(-0.5f, -0.5f, 0)
                * Matrix.CreateOrthographic(20, 20, maxHeight, maxHeight * 3)
                * Matrix.CreateScale(1, (float)Math.Sqrt(2), 1);

            UpdateMatrices();
        }

        public bool HandleKeyboardInput()
        {
            bool modified = false;

            KeyboardState keyboard = Keyboard.GetState();

            bool up = keyboard.IsKeyDown(Keys.Up);
            bool left = keyboard.IsKeyDown(Keys.Left);
            bool right = keyboard.IsKeyDown(Keys.Right);
            bool down = keyboard.IsKeyDown(Keys.Down);

            if (keyboard.IsKeyDown(Keys.OemPlus) && Zoom < 1)
                UpdateScale(.01f);

            else if (keyboard.IsKeyDown(Keys.OemMinus) && Zoom > 0.1f)
                UpdateScale(-.01f);

            if (up)
                UpdatePosition(IsometricCamera.up);

            if (right)
                UpdatePosition(IsometricCamera.right);

            if (left)
                UpdatePosition(IsometricCamera.left);

            if (down)
                UpdatePosition(IsometricCamera.down);

            if (modified)
                UpdateMatrices();

            return modified;

            void UpdatePosition(Vector3 vector)
            {
                target += vector;
                modified = true;
            }

            void UpdateScale(float increment)
            {
                Zoom += increment;
                scaleMatrix.M11 = Zoom;
                scaleMatrix.M22 = Zoom;

                modified = true;
            }
        }

        private void UpdateMatrices()
        {
            viewMatrix = Matrix.CreateLookAt(target + positionFromOrigin, target, Vector3.Up);
            MatrixUtilities.Multiply(in viewMatrix, in scaleMatrix, out viewMatrix);

            MatrixUtilities.Multiply(in worldMatrix, in viewMatrix, out worldViewMatrix);
            MatrixUtilities.Multiply(in worldViewMatrix, in projectionMatrix, out worldViewProjectionMatrix);

            MatrixUtilities.Invert(in worldViewProjectionMatrix, out invertedProjectionMatrix);
        }

        public void Test(GraphicsDevice device)
        {
            device.DrawUserPrimitives(PrimitiveType.TriangleList, PointXY(target, Color.Red), 0, 2);
            device.DrawUserPrimitives(PrimitiveType.TriangleList, PointXZ(target, Color.Yellow), 0, 2);
            device.DrawUserPrimitives(PrimitiveType.TriangleList, PointYZ(target, Color.Green), 0, 2);
        }

        private VertexPositionColor[] PointXY(Vector3 v, Color color)
        {
            return new VertexPositionColor[]
            {
                new(Vector3.Add(v, new Vector3(0, 0, 0)), color),
                new(Vector3.Add(v, new Vector3(0, 1, 0)), color),
                new(Vector3.Add(v, new Vector3(1, 1, 0)), color),
                new(Vector3.Add(v, new Vector3(1, 1, 0)), color),
                new(Vector3.Add(v, new Vector3(1, 0, 0)), color),
                new(Vector3.Add(v, new Vector3(0, 0, 0)), color)
            };
        }

        private VertexPositionColor[] PointXZ(Vector3 v, Color color)
        {
            return new VertexPositionColor[]
            {
                new(Vector3.Add(v, new Vector3(0, 0, 1)), color),
                new(Vector3.Add(v, new Vector3(0, 0, 0)), color),
                new(Vector3.Add(v, new Vector3(1, 0, 0)), color),
                new(Vector3.Add(v, new Vector3(1, 0, 0)), color),
                new(Vector3.Add(v, new Vector3(1, 0, 1)), color),
                new(Vector3.Add(v, new Vector3(0, 0, 1)), color),
            };
        }

        private VertexPositionColor[] PointYZ(Vector3 v, Color color)
        {
            return new VertexPositionColor[]
            {
                new(Vector3.Add(v, new Vector3(0, 0, 0)), color),
                new(Vector3.Add(v, new Vector3(0, 0, 1)), color),
                new(Vector3.Add(v, new Vector3(0, 1, 1)), color),
                new(Vector3.Add(v, new Vector3(0, 1, 1)), color),
                new(Vector3.Add(v, new Vector3(0, 1, 0)), color),
                new(Vector3.Add(v, new Vector3(0, 0, 0)), color)
            };
        }
    }
}
