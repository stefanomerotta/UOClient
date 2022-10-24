using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace UOClient
{
    public sealed class IsometricCamera
    {
        private Vector3 position;
        private Vector3 target;
        private float zoom;

        public Matrix WorldMatrix { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }

        public Vector3 Target => target;
        public Vector3 Position => position;

        public IsometricCamera()
        {
            float val = (float)Math.Cos(Math.PI / 4);

            position = new Vector3(val, val * 10, val);
            target = Vector3.Zero;
            zoom = 1;

            WorldMatrix = Matrix.Identity;
            ViewMatrix = Matrix.CreateLookAt(position, target, Vector3.UnitY);

            ProjectionMatrix = Matrix.CreateOrthographic(20, 20, 0, 3000.0f);
            //* Matrix.CreateScale(1, 1.5f, 1);
        }

        public void HandleKeyboardInput()
        {
            KeyboardState keyboard = Keyboard.GetState();

            bool up = keyboard.IsKeyDown(Keys.Up);
            bool left = keyboard.IsKeyDown(Keys.Left);
            bool right = keyboard.IsKeyDown(Keys.Right);
            bool down = keyboard.IsKeyDown(Keys.Down);

            if (keyboard.IsKeyDown(Keys.OemPlus))
                zoom += .01f;
            else if (keyboard.IsKeyDown(Keys.OemMinus))
                zoom -= .01f;

            if (up)
            {
                position.X -= 1;
                position.Z -= 1;
                target.X -= 1;
                target.Z -= 1;
            }

            if (right)
            {
                position.X += 1;
                position.Z -= 1;
                target.X += 1;
                target.Z -= 1;
            }

            if (left)
            {
                position.X -= 1;
                position.Z += 1;
                target.X -= 1;
                target.Z += 1;
            }

            if (down)
            {
                position.X += 1;
                position.Z += 1;
                target.X += 1;
                target.Z += 1;
            }

            ViewMatrix = Matrix.CreateLookAt(position, target, Vector3.UnitY)
                * Matrix.CreateScale(zoom, zoom, 1);
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
