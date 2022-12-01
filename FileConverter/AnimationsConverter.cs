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
        private readonly AnimationsLoaderUOP[] animationsLoadersUOP;
        private readonly AnimationsLoaderMUL[] animationsLoadersMUL;

        public AnimationsConverter(string filePath)
        {
            animationsLoadersUOP = new AnimationsLoaderUOP[5];
            animationsLoadersUOP[4] = AnimationsLoaderUOP.Empty;

            Enumerable.Range(1, 4)
                .AsParallel()
                .Select(id => new AnimationsLoaderUOP(filePath, id))
                .ToArray()
                .CopyTo(animationsLoadersUOP, 0);

            animationsLoadersMUL = new AnimationsLoaderMUL[]
            {
                new(filePath, 1),
                new(filePath, 2),
                new(filePath, 3),
                new(filePath, 4),
                new(filePath, 5)
            };
        }

        public void Convert(string filePath, string fileName)
        {
            Enumerable.Range(0, animationsLoadersUOP.Length)
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(i => ConvertWithLoader(filePath, fileName, i));
        }

        private void ConvertWithLoader(string filePath, string fileName, int loaderIndex)
        {
            int fileId = loaderIndex + 1;
            List<MappingEntry> loadedAnims = new();

            using FileStream fileStream = File.Create(Path.Combine(filePath, string.Format(fileName, fileId)));
            using PackageWriter<AnimContentMetadata> writer = new(fileStream);

            AnimationsLoaderMUL loaderMUL = animationsLoadersMUL[loaderIndex];
            AnimationsLoaderUOP loaderUOP = animationsLoadersUOP[loaderIndex];

            int maxAnimId = Math.Max(loaderMUL.MaxAnimId, loaderUOP.MaxAnimId);
            byte[] dataBuffer = Array.Empty<byte>();

            int index = 0;
            ushort lastLoadedAnim = ushort.MaxValue;

            for (ushort animId = 0; animId <= maxAnimId; animId++)
            {
                bool mulValid;
                bool uopValid;

                for (byte actionId = 0; actionId < AnimationsLoaderUOP.MaxActionsPerAnimation; actionId++)
                {
                    Span<AnimFrame[]> mulFrames = loaderMUL.LoadAnimation(animId, actionId);
                    mulValid = ValidateActionFrames(mulFrames);

                    Span<AnimFrame[]> uopFrames = loaderUOP.LoadAnimation(animId, actionId);
                    uopValid = ValidateActionFrames(uopFrames);

                    if (!mulValid && !uopValid)
                        continue;

                    if (lastLoadedAnim != animId)
                    {
                        lastLoadedAnim = animId;
                        loadedAnims.Add(new(animId, uopValid, mulValid));
                    }

                    Span<AnimFrame[]> frames = uopValid ? uopFrames: mulFrames;

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

            Span<MappingEntry> span = CollectionsMarshal.AsSpan(loadedAnims);

            for (int i = 0; i < loadedAnims.Count; i++)
            {
                MappingEntry entry = span[i];

                mappingWriter.WriteLine($"{entry.AnimId} - {(entry.MUL ? "MUL" : "   ")} {(entry.UOP ? "UOP" : "   ")}");
            }

            static bool ValidateActionFrames(Span<AnimFrame[]> frames)
            {
                if (frames.Length != 5)
                    return false;

                for (byte direction = 0; direction < 5; direction++)
                {
                    if (frames[direction] is not { Length: > 0 })
                        return false;
                }

                return true;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < animationsLoadersUOP.Length; i++)
            {
                animationsLoadersUOP[i].Dispose();
            }
        }

        private record struct MappingEntry(ushort AnimId, bool UOP, bool MUL);
    }
}
