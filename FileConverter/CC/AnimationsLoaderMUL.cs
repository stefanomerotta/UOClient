using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using FileConverter.CC.Utilities;
using GameData.Structures.Contents.Animations;
using System.Runtime.InteropServices;

namespace FileConverter.CC
{
    internal sealed class AnimationsLoaderMUL : IDisposable
    {
        private static readonly BcEncoder ddsEncoder;

        static AnimationsLoaderMUL()
        {
            ddsEncoder = new();

            ddsEncoder.OutputOptions.FileFormat = OutputFileFormat.Dds;
            ddsEncoder.OutputOptions.Format = CompressionFormat.Bc3;
            ddsEncoder.OutputOptions.GenerateMipMaps = false;
            ddsEncoder.OutputOptions.Quality = CompressionQuality.BestQuality;
        }

        private readonly IndexEntry[] entries;
        private readonly BinaryReader reader;

        public int MaxAnimId => entries.Length - 1;

        public AnimationsLoaderMUL(string filePath, int id)
        {
            string idxName = $"anim{id}.idx";
            string mulName = $"anim{id}.mul";

            if (id == 1)
            {
                idxName = "anim.idx";
                mulName = "anim.mul";
            }

            FileStream mulStream = File.OpenRead(Path.Combine(filePath, mulName));
            reader = new(mulStream);

            using FileStream idxStream = File.OpenRead(Path.Combine(filePath, idxName));
            using BinaryReader idxReader = new(idxStream);

            entries = new IndexEntry[idxReader.BaseStream.Length / 12];

            for (int i = 0; i < entries.Length; i++)
            {
                IndexEntry entry = idxReader.Read<IndexEntry>();
                idxReader.Skip(4);

                if (entry is not { Offset: >= 0, Size: > 0 })
                    continue;

                entries[i] = entry;
            }
        }

        public Span<AnimFrame[]> LoadAnimation(ushort animId, byte actionId)
        {
            IndexEntry entry = entries[animId];

            if (entry.Size == 0)
                return Span<AnimFrame[]>.Empty;

            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

            Span<ushort> palette = stackalloc ushort[256];
            reader.Read(MemoryMarshal.AsBytes(palette));

            long dataStart = reader.BaseStream.Position;
            uint frameCount = reader.ReadUInt32();

            if(frameCount < 5)
                return Span<AnimFrame[]>.Empty;

            Span<int> frameOffsets = stackalloc int[(int)frameCount];
            reader.Read(MemoryMarshal.AsBytes(frameOffsets));

            AnimFrame[][] frames = new AnimFrame[5][];
            uint[] dataBuffer = Array.Empty<uint>();

            int directionFrameCount = (int)frameCount / 5;
            AnimFrame[] directionFrames = frames[0] = new AnimFrame[directionFrameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                ref AnimFrame frame = ref directionFrames[frameIndex % directionFrameCount];

                reader.BaseStream.Seek(dataStart + frameOffsets[frameIndex], SeekOrigin.Begin);

                if (!ConvertFrame(reader, palette, ref dataBuffer, ref frame))
                    throw new Exception("Invalid animation");
            }

            return frames;
        }

        private static bool ConvertFrame(BinaryReader reader, Span<ushort> palette, ref uint[] dataBuffer, ref AnimFrame frame)
        {
            ref AnimFrameHeader frameHeader = ref frame.Header;

            frameHeader.CenterX = reader.ReadInt16();
            frameHeader.CenterY = reader.ReadInt16();
            frameHeader.Width = reader.ReadInt16();
            frameHeader.Height = reader.ReadInt16();

            if (frameHeader.Width <= 0 || frameHeader.Height <= 0)
            {
                frame.Data = Array.Empty<byte>();
                return false;
            }

            int dataLength = frameHeader.Width * frameHeader.Height;
            frameHeader.DataLength = (ushort)dataLength;

            if (dataBuffer.Length < dataLength)
                dataBuffer = new uint[dataLength];

            uint header = reader.ReadUInt32();

            while (header != 0x7FFF7FFF && reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ushort runLength = (ushort)(header & 0x0FFF);
                int x = (int)((header >> 22) & 0x03FF);

                if ((x & 0x0200) > 0)
                    x |= unchecked((int)0xFFFFFE00);

                int y = (int)((header >> 12) & 0x3FF);

                if ((y & 0x0200) > 0)
                    y |= unchecked((int)0xFFFFFE00);

                x += frameHeader.CenterX;
                y += frameHeader.CenterY + frameHeader.Height;

                int block = y * frameHeader.Width + x;

                for (int k = 0; k < runLength; ++k, ++block)
                {
                    ushort val = palette[reader.ReadByte()];

                    if (val != 0)
                        dataBuffer[block] = HuesUtility.From16To32(val) | 0xFF_00_00_00;
                    else
                        dataBuffer[block] = 0;
                }

                header = reader.ReadUInt32();
            }

            frame.Data = ddsEncoder.EncodeToRawBytes(MemoryMarshal.AsBytes(dataBuffer.AsSpan(0, dataLength)),
                frameHeader.Width, frameHeader.Height, PixelFormat.Rgba32, 0, out _, out _);

            return true;
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct IndexEntry
        {
            public readonly int Offset;
            public readonly int Size;
        }
    }
}
