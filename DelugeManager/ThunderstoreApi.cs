using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DelugeManager;

public static class ThunderstoreApi
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static string PackageProvider { get; set; } = "https://thunderstore.io";

    private static readonly PackageIndex packageIndex = new();

    public static async Task VerifyPackageList()
    {
        using var ms = await Program.ReadOrDownloadCache(Path.Combine(Program.CacheFolder, "modlist.json"), "c/riskofrain2/api/v1/package/", $"package list from {PackageProvider}");

        ms.Position = 0;
        var packageList = await JsonSerializer.DeserializeAsync<Package[]>(ms, SerializerOptions);
        if(packageList != null)
            packageIndex.Packages = packageList;
    }

    public static async Task<PackageVersion> GetPackageAsync(string packageName, string packageNamespace, RoR2Version? targetVersion = null, string? packageVersion = null)
    {
        await VerifyPackageList();

        if(packageIndex.Packages.Length == 0)
            return null;

        var package = packageIndex.Packages.First(p => p.Name == packageName && p.Owner == packageNamespace);
        if(package == null)
            return null;

        if(targetVersion == null)
        {
            if(packageVersion == null)
                return package.Versions[0];

            return package.Versions.First(v => v.VersionNumber == packageVersion);
        }

        PackageVersion ver = null;
        var oldestDate = new DateTimeOffset(targetVersion.Value.Date.Value, TimeOnly.MinValue, TimeSpan.Zero).ToUniversalTime();
        var newestDate = new DateTimeOffset(RoR2Versions.Versions.GetByID(targetVersion.Value.NextMajorVersion).Date.Value, TimeOnly.MinValue, TimeSpan.Zero).ToUniversalTime();

        int ind = 0;
        if(packageVersion != null)
        {
            for(int i = 0; i < package.Versions.Length; i++)
            {
                if(package.Versions[i].VersionNumber == packageVersion)
                {
                    ver = package.Versions[i];
                    ind = i;
                    break;
                }
            }
        }

        // search for latest version for target game version
        for(int i = ind; i < package.Versions.Length; i++)
        {
            PackageVersion version = package.Versions[i];

            List<string> categories = [..package.Categories];

            if(version.DateCreated.ToUniversalTime() >= oldestDate && version.DateCreated.ToUniversalTime() < newestDate)
                continue;

            if(categories.Contains("Surivors of the Void") && targetVersion.Value.Date >= RoR2Versions.Versions.GetByID("2024_05_20_Update_Devotion").Date)
                continue;

            ver = version;
            break;
        }

        return ver;
    }
}
