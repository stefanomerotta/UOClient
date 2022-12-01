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
    internal sealed class AnimationsConverter : IDisposable
    {
        private readonly AnimationSequence animationSequence;
        private readonly AnimationsLoader[] animationsLoaders;

        public AnimationsConverter(string filePath)
        {
            animationSequence = new(filePath);

            animationsLoaders = Enumerable.Range(1, 4)
                .AsParallel()
                .Select(id => new AnimationsLoader(filePath, id))
                .ToArray();
        }

        public void Convert(string filePath, string fileName)
        {
            Enumerable.Range(0, animationsLoaders.Length)
                .AsParallel()
                .ForAll(i => ConvertWithLoader(filePath, fileName, i));
        }

        private void ConvertWithLoader(string filePath, string fileName, int loaderIndex)
        {
            int fileId = loaderIndex + 1;
            List<ushort> loadedAnims = new();

            using FileStream fileStream = File.Create(Path.Combine(filePath, string.Format(fileName, fileId)));
            using PackageWriter<AnimContentMetadata> writer = new(fileStream);

            int maxAnimId = animationsLoaders.Select(loader => loader.MaxAnimId).Max();
            byte[] dataBuffer = Array.Empty<byte>();

            int index = 0;
            ushort lastLoadedAnim = ushort.MaxValue;

            for (ushort animId = 0; animId <= maxAnimId; animId++)
            {
                AnimationsLoader loader = animationsLoaders[loaderIndex];

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

                    if (lastLoadedAnim != animId)
                    {
                        lastLoadedAnim = animId;
                        loadedAnims.Add(animId);
                    }

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

            using FileStream mappingStream = File.Create(Path.Combine(filePath, $"mappings{fileId}.txt"));
            using StreamWriter mappingWriter = new(mappingStream);

            Span<ushort> span = CollectionsMarshal.AsSpan(loadedAnims);

            for (int i = 0; i < loadedAnims.Count; i++)
            {
                mappingWriter.WriteLine(span[i]);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < animationsLoaders.Length; i++)
            {
                animationsLoaders[i].Dispose();
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
