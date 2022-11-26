using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using Common.Buffers;
using FileConverter.CC.Utilities;
using GameData.Enums;
using Microsoft.Toolkit.HighPerformance;
using Mythic.Package;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileConverter.CC
{
    internal struct FrameInfo
    {
        public int Num;
        public short CenterX;
        public short CenterY;
        public short Width;
        public short Height;
        public uint[] Pixels;
    }

    internal class AnimationGroupUop
    {
        public uint CompressedLength;
        public uint DecompressedLength;
        public int FileIndex;
        public uint Offset;
    }

    internal sealed class AnimationsLoader
    {
        private const int maxAnimationsCount = 2048;
        private const int maxActionsPerAnimation = 80;

        private readonly AnimationSequence animationSequence;
        private readonly AnimEntry[] animations;

        public AnimationsLoader(string filePath, int id, AnimationSequence animationSequence)
        {
            this.animationSequence = animationSequence;

            MythicPackage package = new(Path.Combine(filePath, $"AnimationFrame{id}.uop"));

            List<FileEntry> list = new(package.Blocks.Count * maxAnimationsCount);

            int maxAnimId = 0;
            for (int animId = 0; animId < maxAnimationsCount; animId++)
            {
                for (int actionId = 0; actionId < maxActionsPerAnimation; actionId++)
                {
                    SearchResult result = package.SearchExactFileName($"build/animationlegacyframe/{animId:D6}/{actionId:D2}.bin");
                    if (!result.Found || result.File.DecompressedSize <= 0)
                        continue;

                    list.Add(new(animId, actionId, result.File));
                    maxAnimId = animId;
                }
            }

            animations = new AnimEntry[maxAnimId + 1];

            Span<FileEntry> span = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < span.Length; i++)
            {
                ref FileEntry entry = ref span[i];
                ref AnimEntry anim = ref animations[entry.AnimId];

                if (anim.Files is null)
                    anim = new();

                Debug.Assert(anim.Files[entry.ActionId] is null);

                anim.Files[entry.ActionId] = entry.File;
            }
        }

        public void LoadAnimation(int animId)
        {
            ref AnimEntry entry = ref animations[animId];

            MythicPackageFile[] actions = entry.Files;
            if (actions is null)
                return;

            for (byte actionId = 0; actionId < maxActionsPerAnimation; actionId++)
            {
                MythicPackageFile file = actions[actionId];
                if (file is null)
                    continue;

                byte[] bytes = file.Unpack();

                ByteSpanReader reader = new(bytes);
                reader.Advance(32);

                int frameCount = reader.ReadInt32();
                uint dataStart = reader.ReadUInt32();

                reader.Seek((int)dataStart);
                ReadOnlySpan<FrameHeader> headers = reader.ReadSpan<FrameHeader>(frameCount);

                int headerIndex = 0;
                for (int frameIndex = 0; frameIndex < frameCount;)
                {
                    ref readonly FrameHeader header = ref headers[headerIndex];

                    reader.Seek((int)(dataStart + (headerIndex * Unsafe.SizeOf<FrameHeader>()) + header.DataOffset));
                    headerIndex++;

                    int zeroBasedFrameId = header.FrameId - 1;

                    Debug.Assert(header.ActionId == actionId);

                    if (frameIndex > zeroBasedFrameId)
                        throw new Exception($"Unexpected frameId {header.FrameId}");

                    else if (frameIndex < zeroBasedFrameId)
                        frameIndex = zeroBasedFrameId;

                    else
                        frameIndex++;

                    FrameInfo frame = new();
                    ReadSpriteData(ref reader, ref frame, animId, actionId, frameIndex - 1);
                }
            }
        }

        private static void ReadSpriteData(ref ByteSpanReader reader, ref FrameInfo frame, int animId, int actionId, int frameId)
        {
            ReadOnlySpan<ushort> palette = reader.ReadSpan(512).Cast<byte, ushort>();

            frame.CenterX = reader.ReadInt16();
            frame.CenterY = reader.ReadInt16();
            frame.Width = reader.ReadInt16();
            frame.Height = reader.ReadInt16();

            if (frame.Width <= 0 || frame.Height <= 0)
                return;

            frame.Pixels = new uint[frame.Width * frame.Height];

            Span<uint> data = frame.Pixels;

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

                x += frame.CenterX;
                y += frame.CenterY + frame.Height;

                int block = y * frame.Width + x;

                for (int k = 0; k < runLength; ++k, ++block)
                {
                    ushort val = palette[reader.Read()];

                    if (val != 0)
                        data[block] = HuesUtility.From16To32(val) | 0xFF_00_00_00;
                    else
                        data[block] = 0;
                }

                header = reader.ReadUInt32();
            }

            using FileStream file = File.Create($"C:\\Test\\{animId:D4}-{actionId:D2}-{frameId:D3}.dds");

            BcEncoder encoder = new();
            encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;
            encoder.OutputOptions.Format = CompressionFormat.Bc3;
            encoder.OutputOptions.GenerateMipMaps = false;
            encoder.OutputOptions.Quality = CompressionQuality.BestQuality;

            encoder.EncodeToStream(MemoryMarshal.AsBytes(data), frame.Width, frame.Height, PixelFormat.Rgba32, file);
        }

        //private void LoadUop()
        //{
        //    for (ushort animID = 0; animID < AnimationsLoader.Instance.MAX_ANIMATIONS_DATA_INDEX_COUNT; animID++)
        //    {
        //        for (byte grpID = 0; grpID < MAX_ACTIONS; grpID++)
        //        {
        //            string hashstring = $"build/animationlegacyframe/{animID:D6}/{grpID:D2}.bin";
        //            ulong hash = UOFileUop.CreateHash(hashstring);

        //            for (int i = 0; i < _filesUop.Length; i++)
        //            {
        //                UOFileUop uopFile = _filesUop[i];

        //                if (uopFile != null && uopFile.TryGetUOPData(hash, out UOFileIndex data))
        //                {
        //                    if (_dataIndex[animID] == null)
        //                    {
        //                        _dataIndex[animID] = new IndexAnimation
        //                        {
        //                            UopGroups = new AnimationGroupUop[MAX_ACTIONS]
        //                        };
        //                    }

        //                    _dataIndex[animID].InitializeUOP();

        //                    ref AnimationGroupUop g = ref _dataIndex[animID].UopGroups[grpID];

        //                    g = new AnimationGroupUop
        //                    {
        //                        Offset = (uint)data.Offset,
        //                        CompressedLength = (uint)data.Length,
        //                        DecompressedLength = (uint)data.DecompressedLength,
        //                        FileIndex = i,
        //                    };
        //                }
        //            }
        //        }
        //    }


        //    for (int i = 0; i < _filesUop.Length; i++)
        //    {
        //        _filesUop[i]?.ClearHashes();
        //    }

        //    string animationSequencePath = UOFileManager.GetUOFilePath("AnimationSequence.uop");

        //    if (!File.Exists(animationSequencePath))
        //    {
        //        Log.Warn("AnimationSequence.uop not found");

        //        return;
        //    }

        //    UOFileUop animSeq = new UOFileUop(animationSequencePath, "build/animationsequence/{0:D8}.bin");
        //    UOFileIndex[] animseqEntries = new UOFileIndex[Math.Max(animSeq.TotalEntriesCount, AnimationsLoader.Instance.MAX_ANIMATIONS_DATA_INDEX_COUNT)];
        //    animSeq.FillEntries(ref animseqEntries);

        //    Span<byte> spanAlloc = stackalloc byte[1024];

        //    for (int i = 0; i < animseqEntries.Length; i++)
        //    {
        //        ref UOFileIndex entry = ref animseqEntries[i];

        //        if (entry.Offset == 0)
        //        {
        //            continue;
        //        }

        //        animSeq.Seek(entry.Offset);


        //        byte[] buffer = null;

        //        Span<byte> span = entry.DecompressedLength <= 1024 ? spanAlloc : (buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(entry.DecompressedLength));

        //        try
        //        {
        //            fixed (byte* destPtr = span)
        //            {
        //                ZLib.Decompress
        //                (
        //                    animSeq.PositionAddress,
        //                    entry.Length,
        //                    0,
        //                    (IntPtr)destPtr,
        //                    entry.DecompressedLength
        //                );
        //            }

        //            StackDataReader reader = new StackDataReader(span.Slice(0, entry.DecompressedLength));

        //            uint animID = reader.ReadUInt32LE();
        //            reader.Skip(48);
        //            int replaces = reader.ReadInt32LE();

        //            if (replaces != 48 && replaces != 68)
        //            {
        //                for (int k = 0; k < replaces; k++)
        //                {
        //                    int oldGroup = reader.ReadInt32LE();
        //                    uint frameCount = reader.ReadUInt32LE();
        //                    int newGroup = reader.ReadInt32LE();

        //                    if (frameCount == 0 && _dataIndex[animID] != null)
        //                    {
        //                        _dataIndex[animID].ReplaceUopGroup((byte)oldGroup, (byte)newGroup);
        //                    }

        //                    reader.Skip(60);
        //                }

        //                if (_dataIndex[animID] != null)
        //                {
        //                    if (animID == 0x04E7 || animID == 0x042D || animID == 0x04E6 || animID == 0x05F7)
        //                    {
        //                        _dataIndex[animID].MountedHeightOffset = 18;
        //                    }
        //                    else if (animID == 0x01B0 || animID == 0x0579 || animID == 0x05F6 || animID == 0x05A0)
        //                    {
        //                        _dataIndex[animID].MountedHeightOffset = 9;
        //                    }
        //                }
        //            }

        //            reader.Release();
        //        }
        //        finally
        //        {
        //            if (buffer != null)
        //            {
        //                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        //            }
        //        }
        //    }

        //    animSeq.Dispose();
        //}

        //private Span<FrameInfo> ReadUOPAnimationFrames(ushort animID, byte animGroup, byte direction)
        //{
        //    AnimationGroupUop animData = _dataIndex[animID]?.UopGroups?[animGroup];

        //    if (animData == null)
        //    {
        //        return Span<FrameInfo>.Empty;
        //    }

        //    if (_frames == null)
        //    {
        //        _frames = new FrameInfo[22];
        //    }

        //    if (animData.FileIndex < 0 || animData.FileIndex >= _filesUop.Length)
        //    {
        //        return _frames.AsSpan().Slice(0, 0);
        //    }

        //    if (animData.FileIndex == 0 && animData.CompressedLength == 0 && animData.DecompressedLength == 0 && animData.Offset == 0)
        //    {
        //        Log.Warn("uop animData is null");

        //        return _frames.AsSpan().Slice(0, 0);
        //    }

        //    int decLen = (int)animData.DecompressedLength;
        //    UOFileUop file = _filesUop[animData.FileIndex];
        //    file.Seek(animData.Offset);

        //    if (_decompressedData == null || decLen > _decompressedData.Length)
        //    {
        //        _decompressedData = new byte[decLen];
        //    }

        //    fixed (byte* ptr = _decompressedData.AsSpan())
        //    {
        //        ZLib.Decompress
        //        (
        //            file.PositionAddress,
        //            (int)animData.CompressedLength,
        //            0,
        //            (IntPtr)ptr,
        //            decLen
        //        );
        //    }

        //    StackDataReader reader = new StackDataReader(_decompressedData.AsSpan().Slice(0, decLen));
        //    reader.Skip(32);

        //    long end = (long)reader.StartAddress + reader.Length;

        //    int fc = reader.ReadInt32LE();
        //    uint dataStart = reader.ReadUInt32LE();
        //    reader.Seek(dataStart);

        //    ANIMATION_GROUPS_TYPE type = _dataIndex[animID].Type;
        //    byte frameCount = (byte)(type < ANIMATION_GROUPS_TYPE.EQUIPMENT ? Math.Round(fc / 5f) : 10);
        //    if (frameCount > _frames.Length)
        //    {
        //        _frames = new FrameInfo[frameCount];
        //    }

        //    Span<FrameInfo> frames = _frames.AsSpan().Slice(0, frameCount);

        //    /* If the UOP files didn't omit frames, we could just do this:
        //     * reader.Skip(sizeof(UOPAnimationHeader) * direction * frameCount);
        //     * but we can't. So we have to walk through the frames to seek to where we need to go.
        //     */
        //    UOPAnimationHeader* animHeaderInfo = (UOPAnimationHeader*)reader.PositionAddress;

        //    for (ushort currentDir = 0; currentDir <= direction; currentDir++)
        //    {
        //        for (ushort frameNum = 0; frameNum < frameCount; frameNum++)
        //        {
        //            long start = reader.Position;
        //            animHeaderInfo = (UOPAnimationHeader*)reader.PositionAddress;

        //            if (animHeaderInfo->Group != animGroup)
        //            {
        //                /* Something bad has happened. Just return. */
        //                return _frames.AsSpan().Slice(0, 0);
        //            }

        //            /* FrameID is 1's based and just keeps increasing, regardless of direction.
        //             * So north will be 1-22, northeast will be 23-44, etc. And it's possible for frames
        //             * to be missing. */
        //            ushort headerFrameNum = (ushort)((animHeaderInfo->FrameID - 1) % frameCount);

        //            ref var frame = ref frames[frameNum];

        //            // we need to zero-out the frame or we will see ghost animations coming from other animation queries
        //            frame.Num = frameNum;
        //            frame.CenterX = 0;
        //            frame.CenterY = 0;
        //            frame.Width = 0;
        //            frame.Height = 0;

        //            if (frameNum < headerFrameNum)
        //            {
        //                /* Missing frame. Keep walking forward. */
        //                continue;
        //            }

        //            if (frameNum > headerFrameNum)
        //            {
        //                /* We've reached the next direction early */
        //                break;
        //            }

        //            if (currentDir == direction)
        //            {
        //                /* We're on the direction we actually wanted to read */
        //                if (start + animHeaderInfo->DataOffset >= reader.Length)
        //                {
        //                    /* File seems to be corrupt? Skip loading. */
        //                    continue;
        //                }

        //                reader.Skip((int)animHeaderInfo->DataOffset);

        //                ushort* palette = (ushort*)reader.PositionAddress;
        //                reader.Skip(512);

        //                ReadSpriteData(ref reader, palette, ref frame, true);
        //            }

        //            reader.Seek(start + sizeof(UOPAnimationHeader));
        //        }
        //    }

        //    reader.Release();

        //    return frames;
        //}

        private readonly struct FileEntry
        {
            public readonly int AnimId;
            public readonly int ActionId;
            public readonly MythicPackageFile File;

            public FileEntry(int animId, int actionId, MythicPackageFile file)
            {
                AnimId = animId;
                ActionId = actionId;
                File = file;
            }
        }

        private readonly struct AnimEntry
        {
            public readonly MythicPackageFile[] Files;

            public AnimEntry()
            {
                Files = new MythicPackageFile[maxActionsPerAnimation];
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
