using System.Text.Json;
using System.Text.Json.Serialization;

namespace DelugeManager;

public class RoR2Profile
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = {
            new JsonStringEnumConverter(),
        },
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };

    public IList<RoR2Plugin> Mods { get; } = [];

    public string GameVersion { get; set; } = RoR2Versions.Latest.Identifier;

    public string Name { get; set; }

    public string LaunchArguments { get; set; } = "";
}
