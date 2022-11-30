using Common.Buffers;
using MYPReader;
using System.Collections;

namespace FileConverter.CC
{
    internal sealed class AnimationSequence
    {
        private const int animMaxCount = ushort.MaxValue;

        private readonly BitArray animations;
        private readonly Dictionary<int, Dictionary<int, int>> mappings;

        public AnimationSequence(string filePath)
        {
            animations = new(animMaxCount);
            mappings = new();

            using MythicPackage package = new(Path.Combine(filePath, "animationSequence.uop"));

            foreach (ref readonly MythicPackageFile file in package)
            {
                byte[] bytes = package.UnpackFile(in file);
                ByteSpanReader reader = new(bytes);

                int animId = reader.ReadInt32();
                reader.Advance(48);
                int count = reader.ReadInt32();

                bool any = false;

                for (int i = 0; i < count; i++)
                {
                    int originalAction = reader.ReadInt32();
                    uint frameCount = reader.ReadUInt32();
                    int replacedAction = reader.ReadInt32();
                    reader.Advance(60);

                    if (frameCount != 0)
                        continue;

                    if (!mappings.TryGetValue(animId, out Dictionary<int, int>? actions))
                        actions = new(1);

                    actions.Add(originalAction, replacedAction);

                    any = true;
                }

                animations.Set(animId, any);
            }
        }

        public bool TryGetReplacement(int animId, int actionId, out int replacedActionId)
        {
            replacedActionId = 0;

            if (!animations.Get(animId))
                return false;

            Dictionary<int, int> actions = mappings[animId];
            return actions.TryGetValue(actionId, out replacedActionId);
        }
    }
}
