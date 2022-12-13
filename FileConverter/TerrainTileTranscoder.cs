using FileConverter.Utilities;
using System.Diagnostics;

namespace FileConverter
{
    internal class TerrainTileTranscoder
    {
        private readonly ushort[] ids = new ushort[ushort.MaxValue];

        public TerrainTileTranscoder()
        {
            var grouped = JsonReader.Read<TerrainTranscodeEntry[]>(".\\Content\\TerrainTranscode.json")
                .GroupBy(entry => entry.Id)
                .SelectMany
                (
                    entry => entry.Select(e => e.OldIds), 
                    (group, oldIds) => new TerrainTranscodeEntry(group.Key, oldIds)
                );

            foreach (TerrainTranscodeEntry group in grouped)
            {
                foreach (ushort oldId in group.OldIds)
                {
                    Debug.Assert(ids[oldId] == 0);

                    ids[oldId] = group.Id;
                }
            }
        }

        public ushort GetNewId(ushort oldId)
        {
            return ids[oldId];
        }

        private struct TerrainTranscodeEntry
        {
            public ushort Id;
            public ushort[] OldIds;

            public TerrainTranscodeEntry(ushort id, ushort[] oldIds)
            {
                Id = id;
                OldIds = oldIds;
            }
        }
    }
}
