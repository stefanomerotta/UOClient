using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace UOClient
{
    public sealed class IsometricCamera
    {
        private static readonly Vector3 transformPosition = new Vector3(1, (float)Math.Sqrt(2), 1) * 127;

        private Vector3 target;
        private float zoom;

        public Matrix WorldMatrix { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }

        public Vector3 Position => target + transformPosition;
        public Vector3 Target => target;
        public float Zoom => zoom;

        public IsometricCamera()
        {
            target = new(185, 0, 300);
            zoom = 1;

            WorldMatrix = Matrix.CreateScale(1, .1f, 1);
            
            ProjectionMatrix = Matrix.CreateTranslation(-0.5f, -0.5f, 0)
                * Matrix.CreateOrthographic(20, 20, 0, 3000.0f)
                * Matrix.CreateScale(1, (float)Math.Sqrt(2), 1);

            UpdateViewMatrix();
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
                zoom += .01f;
                modified = true;
            }
            else if (keyboard.IsKeyDown(Keys.OemMinus))
            {
                zoom -= .01f;
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
                UpdateViewMatrix();

            return modified;
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

        private void UpdateViewMatrix()
        {
            ViewMatrix = Matrix.CreateLookAt(target + transformPosition, target, Vector3.UnitY)
                * Matrix.CreateScale(zoom, zoom, 1);
        }
    }
}
