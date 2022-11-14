using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TexturePacker;
using UOClient.Data;

namespace UOClient
{
    internal sealed class TextureBuffer
    {
        private const int maxTextureSize = 8192;

        private static readonly GlobalTextureData[] globalTextures = new GlobalTextureData[ushort.MaxValue];

        private readonly TextureFile textureFile;
        private readonly Dictionary<ushort, int> loadedTextures;
        private readonly List<TextureBufferEntry> entries;

        public TextureBuffer(TextureFile textureFile)
        {
            this.textureFile = textureFile;

            loadedTextures = new();
            entries = new();
        }

        public Rectangle GetOrAdd(ushort id)
        {
            int width;
            int height;

            if (globalTextures[id] == default)
            {
                byte[] data = textureFile.ReadTexture(id, out width, out height);
                globalTextures[id] = new(width, height, data);
            }
            else
            {
                GlobalTextureData data = globalTextures[id];

                width = data.Width;
                height = data.Height;
            }

            Span<TextureBufferEntry> entriesSpan = CollectionsMarshal.AsSpan(entries);

            if (loadedTextures.TryGetValue(id, out int index))
            {
                ref TextureBufferEntry entry = ref entriesSpan[index];
                return entry.Rectangles[id];
            }

            ref TextureBufferEntry current = ref entriesSpan[^1];

            PackedRectangle rect = current.Packer.PackRect(width, height);

            if (rect == default)
            {
                current = new TextureBufferEntry();
                entries.Add(current);

                rect = current.Packer.PackRect(width, height);
            }

            loadedTextures.Add(id, entries.Count - 1);

            return Unsafe.As<PackedRectangle, Rectangle>(ref rect);
        }

        public Texture2D[] CreateTextureAtlases(GraphicsDevice device)
        {
            Texture2D[] textures = new Texture2D[entries.Count];
            
            Span<TextureBufferEntry> entriesSpan = CollectionsMarshal.AsSpan(entries);

            for (int i = 0; i < entriesSpan.Length; i++)
            {
                ref TextureBufferEntry entry = ref entriesSpan[i];

                Texture2D texture = new(device, maxTextureSize, maxTextureSize, false, SurfaceFormat.Dxt5);

                foreach (var pair in entry.Rectangles)
                {
                    ref GlobalTextureData data = ref globalTextures[pair.Key];
                    texture.SetData(0, pair.Value, data.Data, 0, data.Data.Length);
                }

                textures[i] = texture;
            }

            return textures;
        }

        public void Reset()
        {
        }

        private readonly record struct GlobalTextureData
        {
            public readonly ushort Width;
            public readonly ushort Height;
            public readonly byte[] Data;

            public GlobalTextureData(int width, int height, byte[] data)
            {
                Width = (ushort)width;
                Height = (ushort)height;
                Data = data;
            }
        }

        private sealed class TextureBufferEntry : IDisposable
        {
            public readonly Packer Packer;
            public readonly Dictionary<int, Rectangle> Rectangles;

            public TextureBufferEntry()
            {
                Packer = new(maxTextureSize, maxTextureSize);
                Rectangles = new();
            }

            public void Dispose()
            {
                Packer.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
