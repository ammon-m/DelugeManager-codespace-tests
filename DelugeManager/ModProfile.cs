using System.Text.Json;
using System.Text.Json.Serialization;

namespace DelugeManager;

public class ModProfile
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = {
            new JsonStringEnumConverter(),
        },
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public IList<RoR2Plugin> Mods { get; } = [];

    public string GameVersion { get; set; }

    public string Name { get; set; }

    public string LaunchArguments { get; set; } = "";
}
