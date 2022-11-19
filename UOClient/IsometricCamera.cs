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
        public static readonly Vector3 positionFromOrigin = new Vector3(1, (float)Math.Sqrt(2), 1) * 127;

        private Vector3 target;
        private readonly Matrix worldMatrix;
        private readonly Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Matrix scaleMatrix;
        private Matrix worldViewProjection;
        private Matrix invertedProjectionMatrix;

        public ref readonly Matrix WorldMatrix => ref worldMatrix;
        public ref readonly Matrix ViewMatrix => ref viewMatrix;
        public ref readonly Matrix ProjectionMatrix => ref projectionMatrix;
        public ref readonly Matrix ScaleMatrix => ref scaleMatrix;
        public ref readonly Matrix WorldViewProjection => ref worldViewProjection;

        public Vector3 Target => target;
        public float Zoom { get; private set; }

        public IsometricCamera()
        {
            Zoom = 1;

            target = new(710, 0, 1293); //new(835, 0, 904);
            scaleMatrix = Matrix.CreateScale(Zoom, Zoom, 1);

            worldMatrix = Matrix.CreateScale(1, .1f, 1);

            projectionMatrix = Matrix.CreateTranslation(-0.5f, -0.5f, 0)
                * Matrix.CreateOrthographic(20, 20, 0, 3000.0f)
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

            if (keyboard.IsKeyDown(Keys.OemPlus))
            {
                Zoom += .01f;
                UpdateScale();
                modified = true;
            }
            else if (keyboard.IsKeyDown(Keys.OemMinus))
            {
                Zoom -= .01f;
                UpdateScale();
                modified = true;
            }

            if (up)
            {
                target.X -= 1;
                target.Z -= 1;
                modified = true;
            }

            if (right)
            {
                target.X += 1;
                target.Z -= 1;
                modified = true;
            }

            if (left)
            {
                target.X -= 1;
                target.Z += 1;
                modified = true;
            }

            if (down)
            {
                target.X += 1;
                target.Z += 1;
                modified = true;
            }

            if (modified)
                UpdateMatrices();

            return modified;
        }

        private void UpdateScale()
        {
            scaleMatrix.M11 = Zoom;
            scaleMatrix.M22 = Zoom;
        }

        private void UpdateMatrices()
        {
            viewMatrix = Matrix.CreateLookAt(target + positionFromOrigin, target, Vector3.Up);
            MatrixUtilities.Multiply(in viewMatrix, in scaleMatrix, out viewMatrix);

            Matrix worldView = MatrixUtilities.Multiply(in worldMatrix, in viewMatrix);
            MatrixUtilities.Multiply(in worldView, in projectionMatrix, out worldViewProjection);

            MatrixUtilities.Invert(in worldViewProjection, out invertedProjectionMatrix);
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
