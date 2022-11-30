using Common.Buffers;
using FileConverter.CC;
using FileSystem.Enums;
using FileSystem.IO;
using GameData.Structures.Contents.Animations;
using GameData.Structures.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FileConverter
{
    internal sealed class AnimationsConverter
    {
        private readonly AnimationSequence animationSequence;
        private readonly AnimationsLoader[] animationsLoaders;

        public AnimationsConverter(string filePath)
        {
            animationSequence = new(filePath);

            animationsLoaders = new AnimationsLoader[]
            {
                new(filePath, 1),
                new(filePath, 2),
                new(filePath, 3),
                new(filePath, 4),
            };
        }

        public void Convert(string filePath, string fileName)
        {
            List<MappingEntry> mappings = new();

            using FileStream fileStream = File.Create(Path.Combine(filePath, fileName));
            using PackageWriter<AnimContentMetadata> writer = new(fileStream);

            int maxAnimId = animationsLoaders.Select(loader => loader.MaxAnimId).Max();
            byte[] dataBuffer = Array.Empty<byte>();

            int index = 0;
            for (ushort animId = 0; animId <= maxAnimId; animId++)
            {
                MappingEntry entry = new(animId);

                for (int loaderId = 0; loaderId < animationsLoaders.Length; loaderId++)
                {
                    AnimationsLoader loader = animationsLoaders[loaderId];

                    for (byte actionId = 0; actionId < AnimationsLoader.MaxActionsPerAnimation; actionId++)
                    {
                        Span<AnimFrame[]> frames = loader.LoadAnimation(animId, actionId);
                        if (frames.Length != 5)
                            continue;

                        bool validAction = true;
                        for (byte direction = 0; direction < 5; direction++)
                        {
                            if (frames[direction] is not { Length: > 0 })
                            {
                                validAction = false;
                                break;
                            }
                        }

                        if (!validAction)
                            continue;

                        Loaders l = (Loaders)(loaderId + 1);
                        entry.Loaders |= l;

                        if (entry.Loaders != l)
                            continue;

                        for (byte direction = 0; direction < 5; direction++)
                        {
                            AnimFrame[] directionFrames = frames[direction];
                            AnimContentMetadata metadata = new(animId, actionId, direction, (byte)directionFrames.Length);

                            int totalLength = 0;
                            for (int i = 0; i < directionFrames.Length; i++)
                            {
                                totalLength += Unsafe.SizeOf<AnimFrameHeader>();
                                totalLength += directionFrames[i].Data.Length;
                            }

                            if (dataBuffer.Length < totalLength)
                                dataBuffer = new byte[totalLength];

                            ByteSpanWriter bufferWriter = new(dataBuffer);

                            for (int i = 0; i < directionFrames.Length; i++)
                            {
                                ref AnimFrame frame = ref directionFrames[i];

                                bufferWriter.Write(in frame.Header);
                                bufferWriter.Write(frame.Data);
                            }

                            writer.WriteSpan(index++, bufferWriter.WrittenSpan, CompressionAlgorithm.Zstd, in metadata);
                        }
                    }
                }

                mappings.Add(entry);
            }

            using FileStream mappingStream = File.Create(Path.Combine(filePath, "mappings.txt"));
            using StreamWriter mappingWriter = new(mappingStream);

            Span<MappingEntry> span = CollectionsMarshal.AsSpan(mappings);

            for (int i = 0; i < mappings.Count; i++)
            {
                ref MappingEntry entry = ref span[i];

                mappingWriter.WriteLine($"{entry.AnimId:D4} - " +
                    $"{ConvertFlags(entry.Loaders, Loaders.Loader1, 1)} " +
                    $"{ConvertFlags(entry.Loaders, Loaders.Loader2, 2)} " +
                    $"{ConvertFlags(entry.Loaders, Loaders.Loader3, 3)} " +
                    $"{ConvertFlags(entry.Loaders, Loaders.Loader4, 4)}");
            }

            static string ConvertFlags(Loaders loaders, Loaders loader, int id)
            {
                return loaders.HasFlag(loader) ? $"{id}" : " ";
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MappingEntry
        {
            public ushort AnimId;
            public Loaders Loaders;

            public MappingEntry(ushort animId)
            {
                AnimId = animId;
            }
        }

        [Flags]
        private enum Loaders : byte
        {
            None = 0,
            Loader1 = 0x1,
            Loader2 = 0x2,
            Loader3 = 0x4,
            Loader4 = 0x8
        }
    }
}
