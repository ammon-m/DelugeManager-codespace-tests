namespace DelugeManager;

public class RoR2Plugin
{
    public string Name { get; set; }

    public string Namespace { get; set; }

    public Uri? WebsiteURL { get; set; }

    public string? FullName { get; set; }

    public string Description { get; set; } = "";

    public string[] Dependencies { get; set; } = [];

    public string[] Incompatibilities { get; set; } = [];

    public string[] OptionalDependencies { get; set; } = [];

    public string VersionNumber { get; set; } = "-1";

    public bool Enabled { get; set; }
}
