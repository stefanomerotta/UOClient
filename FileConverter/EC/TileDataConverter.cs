using Common.Buffers;
using Common.Utilities;
using FileConverter.EC.Structures;
using GameData.Enums;
using MYPReader;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TileDataExporter.Components;
using RadarColor = GameData.Structures.Contents.Statics.RadarColor;
using ReadOnlyStaticData = GameData.Structures.Contents.Statics.StaticData;

namespace FileConverter.EC
{
    internal sealed partial class TileDataConverter : IDisposable
    {
        private static readonly Regex idRegex = CreateIdRegex();
        private static readonly ReadOnlyStaticData invalidStaticData = new(ushort.MaxValue);

        private readonly MythicPackage package;
        private readonly StringDictionary dictionary;
        private byte[] buffer;

        public TileDataConverter(string path)
        {
            package = new(Path.Combine(path, "tileart.uop"));
            dictionary = new(Path.Combine(path, "string_dictionary.uop"));
            buffer = Array.Empty<byte>();
        }

        public List<ReadOnlyStaticData> ConvertTileData()
        {
            List<ReadOnlyStaticData> converted = new(package.FileCount);

            foreach (ref readonly MythicPackageFile file in package)
            {
                int byteRead = package.UnpackFile(in file, ref buffer);
                converted.Add(LoadFile(buffer.AsSpan(0, byteRead)));
            }

            return converted;
        }

        private ReadOnlyStaticData LoadFile(ReadOnlySpan<byte> bytes)
        {
            ByteSpanReader reader = new(bytes);

            reader.Advance(2);

            TileDataHeader header = reader.Read<TileDataHeader>();

            if (header.TileId >= ushort.MaxValue)
                return invalidStaticData;

            TextureOffset ecOffsets = reader.Read<TextureOffset>();
            TextureOffset ccOffsets = reader.Read<TextureOffset>();

            scoped Span<Property> properties = Span<Property>.Empty;
            int propertiesCount1 = reader.ReadByte();

            if (propertiesCount1 > 0)
            {
                properties = stackalloc Property[propertiesCount1];
                reader.Read(MemoryMarshal.AsBytes(properties));
            }

            int propertiesCount2 = reader.ReadByte();
            reader.Advance(propertiesCount2 * 5);

            int stackAliasCount = reader.ReadInt32();
            reader.Advance(stackAliasCount * 8);

            ReadAppearance(ref reader);

            bool hasSitting = reader.ReadBoolean();
            if (hasSitting)
                reader.Advance(24);

            RadarColor radarColor = reader.Read<RadarColor>();

            bool hasTexture = reader.ReadBoolean();
            if (hasTexture)
                reader.Rewind(1);

            List<ShaderEntry> shaders = ReadTextures(ref reader, hasTexture ? 4 : 2);

            StaticData data = new()
            {
                Id = (ushort)header.TileId,
                Flags = header.Flags1,
                RadarColor = radarColor,
                Properties = new((byte)GetProperty(properties, PropertyKey.Height).Value)
            };

            if (shaders.Count == 0)
                return UnsafeUtility.As<StaticData, ReadOnlyStaticData>(in data);

            /* if true, it has three textures: "wordart", "legacy" and "enhanced"
             * if false, ithas two texture: "legacy" and "enhanced"
             * 
             * "wordart": could be sprite, terrain or water (TODO: handle terrain and water)
             * "legacy" and "enhanced" only sprite
             */

            ShaderEntry ecShader = shaders[1];
            ShaderEntry ccShader = shaders[0];

            if (hasTexture)
            {
                ecShader = shaders[0];
                ccShader = shaders[1];
            }

            TextureEntry ecTexture = ecShader.Textures[0];

            data.Type = GetStaticTypeFromDictionary(in ecShader);

            if (data.Type == StaticTileType.Liquid)
                ecTexture = ecShader.Textures[1];

            data.ECTexture = new
            (
                GetIdFromDictionary(ecTexture.DictionaryIndex),
                (short)ecOffsets.StartX,
                (short)ecOffsets.StartY,
                (short)ecOffsets.EndX,
                (short)ecOffsets.EndY,
                (short)ecOffsets.OffsetX,
                (short)ecOffsets.OffsetY
            );

            TextureEntry ccTexture = ccShader.Textures[0];

            data.CCTexture = new
            (
                GetIdFromDictionary(ccTexture.DictionaryIndex),
                (short)ccOffsets.StartX,
                (short)ccOffsets.StartY,
                (short)ccOffsets.EndX,
                (short)ccOffsets.EndY,
                (short)ccOffsets.OffsetX,
                (short)ccOffsets.OffsetY
            );

            return UnsafeUtility.As<StaticData, ReadOnlyStaticData>(in data);
        }

        private static void ReadAppearance(ref ByteSpanReader reader)
        {
            int appearanceCount = reader.ReadInt32();

            for (int i = 0; i < appearanceCount; i++)
            {
                byte subType = reader.ReadByte();

                if (subType == 1)
                {
                    reader.Advance(5);
                    continue;
                }

                int count = reader.ReadInt32();
                reader.Advance(count * 8);
            }
        }

        private static List<ShaderEntry> ReadTextures(ref ByteSpanReader reader, int count)
        {
            List<ShaderEntry> shaders = new();

            while (reader.RemainingCount > 2 && shaders.Count < count)
            {
                if (reader.ReadByte() != 1)
                    break;

                reader.Advance(1);

                ShaderEntry entry = new()
                {
                    DictionaryShaderIndex = reader.ReadInt32()
                };

                byte textureCount = reader.ReadByte();
                entry.Textures = new TextureEntry[textureCount];

                for (int i = 0; i < textureCount; i++)
                    entry.Textures[i] = ReadTexture(ref reader);

                int count1 = reader.ReadInt32();
                reader.Advance(count1 * 4);

                int count2 = reader.ReadInt32();
                reader.Advance(count2 * 4);

                shaders.Add(entry);
            }

            return shaders;
        }

        private static TextureEntry ReadTexture(ref ByteSpanReader reader)
        {
            TextureEntry entry = new()
            {
                DictionaryIndex = reader.ReadInt32()
            };

            reader.Advance(1);
            entry.TextureStretch = reader.ReadSingle();
            reader.Advance(8);

            return entry;
        }

        private int GetIdFromDictionary(int dictionaryId)
        {
            Match match = idRegex.Match(dictionary.Entries[dictionaryId]);
            if (!match.Success)
                return -1;

            Group group = match.Groups["filename"];
            if (!group.Success)
                return -1;

            return int.Parse(group.Value);
        }

        private StaticTileType GetStaticTypeFromDictionary(in ShaderEntry shader)
        {
            return dictionary.Entries[shader.DictionaryShaderIndex] switch
            {
                "UOSpriteShader" => shader.Textures[0].TextureStretch != 1 ? StaticTileType.Solid : StaticTileType.Static,
                "UOWaterShader" => StaticTileType.Liquid,
                "UOStaticTerrainShader" => StaticTileType.Solid,
                _ => throw new Exception()
            };
        }

        private static Property GetProperty(Span<Property> properties, PropertyKey key)
        {
            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Key == key)
                    return properties[i];

            return default;
        }

        [GeneratedRegex("(?<filename>\\d+)(_.+)*\\.tga", RegexOptions.Compiled)]
        private static partial Regex CreateIdRegex();

        public void Dispose()
        {
            package.Dispose();
        }
    }
}
