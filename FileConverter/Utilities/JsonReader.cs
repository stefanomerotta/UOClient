using GameData.Structures.Contents.Terrains;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileConverter.Utilities
{
    internal static class JsonReader
    {
        public static T Read<T>(string filePath)
        {
            return JsonSerializer.Deserialize<T>
            (
                File.OpenRead(filePath),
                new JsonSerializerOptions()
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    IncludeFields = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                }
            )!;
        }
    }
}
