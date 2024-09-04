namespace DelugeManager;

public class RoR2Plugin
{
    private string authorName = "Unknown";
    private string name = "Unknown";

    public string Name
    {
        get => name;
        set => name = value ?? "Unknown";
    }

    public string AuthorName
    {
        get => authorName;
        set => authorName = value ?? "Unknown";
    }

    public Uri? WebsiteURL { get; set; }

    public string? DisplayName { get; set; }

    public string Description { get; set; } = "";

    public string GameVersion { get; set; } = RoR2Versions.Latest.Identifier;

    public string[] Dependencies { get; set; } = [];

    public string[] Incompatibilities { get; set; } = [];

    public string[] OptionalDependencies { get; set; } = [];

    public Version VersionNumber { get; set; } = new(1, 0, 0);

    public bool Enabled { get; set; }
}
