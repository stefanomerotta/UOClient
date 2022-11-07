using FileConverter.EC.Structures;
using FileConverter.Structures;
using GameData.Enums;
using Mythic.Package;
using System.Text.RegularExpressions;
using TileDataExporter.Components;
using RadarColor = TileDataExporter.Components.RadarColor;

namespace FileConverter.EC
{
    internal class TileDataConverter
    {
        private static readonly Regex idRegex = new(@"(?<filename>\d+)(_.+)*\.tga", RegexOptions.Compiled);
        
        private readonly MythicPackage package;
        private readonly StringDictionary dictionary;

        public TileDataConverter(string path)
        {
            package = new(Path.Combine(path, "tileart.uop"));
            dictionary = new(Path.Combine(path, "string_dictionary.uop"));
        }

        public List<StaticData> ConvertTileData()
        {
            MythicPackageFile[] files = package.Blocks.SelectMany(block => block.Files).ToArray();
            List<StaticData> converted = new(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                converted.Add(LoadFile(files[i]));
            }

            return converted;
        }

        private StaticData LoadFile(MythicPackageFile file)
        {
            byte[] bytes = file.Unpack();

            BinaryReader reader = new(new MemoryStream(bytes));

            reader.Skip(2);

            TileDataHeader header = reader.Read<TileDataHeader>();
            TextureOffset ecOffsets = reader.Read<TextureOffset>();
            TextureOffset ccOffsets = reader.Read<TextureOffset>();

            int propertiesCount1 = reader.ReadByte();
            reader.Skip(propertiesCount1 * 5);

            int propertiesCount2 = reader.ReadByte();
            reader.Skip(propertiesCount2 * 5);

            int stackAliasCount = reader.ReadInt32();
            reader.Skip(stackAliasCount * 8);

            ReadAppearance(reader);

            bool hasSitting = reader.ReadBoolean();
            if (hasSitting)
                reader.Skip(24);

            RadarColor radarColors = reader.Read<RadarColor>();

            bool hasTexture = reader.ReadBoolean();
            if (hasTexture)
                reader.BaseStream.Seek(-1, SeekOrigin.Current);

            List<ShaderEntry> shaders = ReadTextures(reader, hasTexture ? 4 : 2);

            StaticData data = new()
            {
                Id = header.TileId,
                RadarColor = new()
                {
                    R = radarColors.R,
                    G = radarColors.G,
                    B = radarColors.B,
                    A = radarColors.A,
                }
            };

            if (shaders.Count == 0)
                return data;

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

            if (GetStaticTypeFromDictionary(ecShader.DictionaryShaderIndex) == StaticTileType.Liquid)
                ecTexture = ecShader.Textures[1];
            
            data.ECTexture = new()
            {
                Id = GetIdFromDictionary(ecTexture.DictionaryIndex),
                StartX = (short)ecOffsets.StartX,
                StartY = (short)ecOffsets.StartY,
                EndX = (short)ecOffsets.EndX,
                EndY = (short)ecOffsets.EndY,
                OffsetX = (short)ecOffsets.OffsetX,
                OffsetY = (short)ecOffsets.OffsetY
            };
            
            TextureEntry ccTexture = ccShader.Textures[0];

            data.CCTexture = new()
            {
                Id = GetIdFromDictionary(ccTexture.DictionaryIndex),
                StartX = (short)ccOffsets.StartX,
                StartY = (short)ccOffsets.StartY,
                EndX = (short)ccOffsets.EndX,
                EndY = (short)ccOffsets.EndY,
                OffsetX = (short)ccOffsets.OffsetX,
                OffsetY = (short)ccOffsets.OffsetY
            };

            return data;
        }

        private static void ReadAppearance(BinaryReader reader)
        {
            int appearanceCount = reader.ReadInt32();

            for (int i = 0; i < appearanceCount; i++)
            {
                byte subType = reader.ReadByte();

                if (subType == 1)
                {
                    reader.Skip(5);
                    continue;
                }

                int count = reader.ReadInt32();
                reader.Skip(count * 8);
            }
        }

        private static List<ShaderEntry> ReadTextures(BinaryReader reader, int count)
        {
            List<ShaderEntry> shaders = new();

            while (reader.BaseStream.Position < reader.BaseStream.Length - 2 && shaders.Count < count)
            {
                if (reader.ReadByte() != 1)
                    break;

                reader.Skip(1);

                ShaderEntry entry = new()
                {
                    DictionaryShaderIndex = reader.ReadInt32()
                };

                byte textureCount = reader.ReadByte();
                entry.Textures = new TextureEntry[textureCount];

                for (int i = 0; i < textureCount; i++)
                {
                    entry.Textures[i] = ReadTexture(reader);
                }

                int count1 = reader.ReadInt32();
                reader.Skip(count1 * 4);

                int count2 = reader.ReadInt32();
                reader.Skip(count2 * 4);

                shaders.Add(entry);
            }

            return shaders;
        }

        private static TextureEntry ReadTexture(BinaryReader reader)
        {
            TextureEntry entry = new()
            {
                DictionaryIndex = reader.ReadInt32()
            };

            reader.Skip(1);
            entry.TextureStretch = reader.ReadSingle();
            reader.Skip(8);

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

        private StaticTileType GetStaticTypeFromDictionary(int dictionaryId)
        {
            return dictionary.Entries[dictionaryId] switch
            {
                "UOSpriteShader" => StaticTileType.Static,
                "UOWaterShader" => StaticTileType.Liquid,
                "UOStaticTerrainShader" => StaticTileType.Solid,
                _ => throw new Exception()
            };
        }
    }
}
