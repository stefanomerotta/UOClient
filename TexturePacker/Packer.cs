using TexturePacker.Components;
using static TexturePacker.PackerMethods;

namespace TexturePacker
{
    /// <summary>
    /// Simple Packer class that doubles size of the atlas if the place runs out
    /// </summary>
    public unsafe sealed class Packer : IDisposable
    {
        private readonly Context context;

        public int Width => context.Width;
        public int Height => context.Height;

        public Packer(int width = 256, int height = 256)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            // Initialize the context
            int numNodes = width;
            context = new Context(numNodes);

            fixed (Context* contextPtr = &context)
            {
                InitTarget(contextPtr, width, height, context.AllNodes, numNodes);
            }
        }

        public void Dispose()
        {
            context.Dispose();
        }

        /// <summary>
        /// Packs a rect. Returns null, if there's no more place left.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public PackedRectangle PackRect(int width, int height)
        {
            Rect rect = new()
            {
                W = width,
                H = height
            };

            int result;
            fixed (Context* contextPtr = &context)
            {
                result = PackRects(contextPtr, &rect, 1);
            }

            if (result == 0)
                return default;

            return new(rect);
        }
    }
}
