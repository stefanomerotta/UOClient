using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Common.Buffers;
using FileConverter.CC.Utilities;
using GameData.Structures.Contents.Animations;
using Microsoft.Toolkit.HighPerformance;
using MYPReader;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileConverter.CC
{
    internal sealed class AnimationsLoader : IDisposable
    {
        private const int maxAnimationsCount = 2048;
        public const int MaxActionsPerAnimation = 80;

        private static readonly BcEncoder ddsEncoder;

        static AnimationsLoader()
        {
            ddsEncoder = new();

            ddsEncoder.OutputOptions.FileFormat = OutputFileFormat.Dds;
            ddsEncoder.OutputOptions.Format = CompressionFormat.Bc3;
            ddsEncoder.OutputOptions.GenerateMipMaps = false;
            ddsEncoder.OutputOptions.Quality = CompressionQuality.BestQuality;
        }

        private readonly MythicPackage package;
        private readonly AnimEntry[] animations;
        private byte[] buffer;

        public int MaxAnimId => animations.Length - 1;

        public AnimationsLoader(string filePath, int id)
        {
            package = new(Path.Combine(filePath, $"AnimationFrame{id}.uop"));
            buffer = Array.Empty<byte>();

            List<FileEntry> list = new(package.FileCount);

            int maxAnimId = 0;
            for (int animId = 0; animId < maxAnimationsCount; animId++)
            {
                for (int actionId = 0; actionId < MaxActionsPerAnimation; actionId++)
                {
                    ref readonly MythicPackageFile file = ref package.SearchFile($"build/animationlegacyframe/{animId:D6}/{actionId:D2}.bin");
                    if (file.UncompressedSize <= 0)
                        continue;

                    list.Add(new(animId, actionId, file.FileHash));
                    maxAnimId = animId;
                }
            }

            animations = new AnimEntry[maxAnimId + 1];

            Span<FileEntry> span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
            {
                ref FileEntry entry = ref span[i];
                ref AnimEntry anim = ref animations[entry.AnimId];

                if (anim.FileHashes is null)
                    anim = new();

                Debug.Assert(anim.FileHashes[entry.ActionId] == 0);

                anim.FileHashes[entry.ActionId] = entry.FileHash;
            }
        }

        public Span<AnimFrame[]> LoadAnimation(ushort animId, byte actionId)
        {
            if (animId >= animations.Length)
                return Span<AnimFrame[]>.Empty;

            ref AnimEntry entry = ref animations[animId];

            ulong[] actionHashes = entry.FileHashes;
            if (actionHashes is null)
                return Span<AnimFrame[]>.Empty;

            ulong actionHash = actionHashes[actionId];
            if (actionHash == 0)
                return Span<AnimFrame[]>.Empty;

            int byteRead = package.UnpackFile(actionHash, ref buffer);

            ByteSpanReader reader = new(buffer.AsSpan(0, byteRead));
            reader.Advance(32);

            int frameCount = reader.ReadInt32();
            uint dataStart = reader.ReadUInt32();

            AnimFrame[][] frames = new AnimFrame[5][];
            uint[] dataBuffer = Array.Empty<uint>();

            reader.Seek((int)dataStart);
            ReadOnlySpan<FrameHeader> headers = reader.ReadSpan<FrameHeader>(frameCount);

            int currentDirection = 0;
            int directionFrameIndex = 0;
            int directionFrameCount = (int)Math.Ceiling(frameCount / 5d);
            AnimFrame[] directionFrames = frames[0] = new AnimFrame[directionFrameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                ref readonly FrameHeader header = ref headers[frameIndex];
                int zeroBasedFrameId = header.FrameId - 1;

                if (currentDirection != zeroBasedFrameId / directionFrameCount)
                {
                    if (directionFrameIndex < directionFrameCount)
                        Array.Resize(ref frames[currentDirection], directionFrameIndex);

                    if (currentDirection == 4)
                        break;

                    currentDirection++;
                    directionFrames = frames[currentDirection] = new AnimFrame[directionFrameCount];
                    directionFrameIndex = 0;
                }

                ref AnimFrame frame = ref directionFrames[directionFrameIndex];

                reader.Seek((int)(dataStart + (frameIndex * Unsafe.SizeOf<FrameHeader>()) + header.DataOffset));

                Debug.Assert(header.ActionId == actionId);

                if (!ConvertFrame(ref reader, ref dataBuffer, ref frame))
                    continue;

                directionFrameIndex++;
            }

            if (currentDirection != 4)
                return Span<AnimFrame[]>.Empty;

            if (directionFrameIndex < directionFrameCount)
                Array.Resize(ref frames[currentDirection], directionFrameIndex);

            return frames;
        }

        private static bool ConvertFrame(ref ByteSpanReader reader, ref uint[] dataBuffer, ref AnimFrame frame)
        {
            ReadOnlySpan<ushort> palette = reader.ReadSpan(512).Cast<byte, ushort>();

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

            while (header != 0x7FFF7FFF && reader.RemainingCount > 0)
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
            package.Dispose();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct FileEntry
        {
            public readonly int AnimId;
            public readonly int ActionId;
            public readonly ulong FileHash;

            public FileEntry(int animId, int actionId, ulong fileHash)
            {
                AnimId = animId;
                ActionId = actionId;
                FileHash = fileHash;
            }
        }

        private readonly struct AnimEntry
        {
            public readonly ulong[] FileHashes;

            public AnimEntry()
            {
                FileHashes = new ulong[MaxActionsPerAnimation];
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FrameHeader
        {
            public ushort ActionId;
            public ushort FrameId;

            public ushort Unk0;
            public ushort Unk1;
            public ushort Unk2;
            public ushort Unk3;

            public uint DataOffset;
        }
    }
}
