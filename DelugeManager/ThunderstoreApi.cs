using System.Text;
using System.Text.Json;

namespace DelugeManager;

public class ThunderstorePackageExperimental
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static async Task<PackageVersionExperimental> GetPackageAsync(string packageName, string packageNamespace, string packageVersion = null)
    {
        string requestUrl = $"https://thunderstore.io/api/experimental/package/{packageNamespace}/{packageName}";
        if(packageVersion is not null)
            requestUrl += $"/{packageVersion}";

        using var client = new HttpClient();
        using var s = await client.GetStreamAsync(requestUrl);
        using var ms = new MemoryStream();
        await s.CopyToAsync(ms);

        var text = Encoding.UTF8.GetString(ms.GetBuffer());

        if(packageVersion is not null)
            return JsonSerializer.Deserialize<PackageVersionExperimental>(text, SerializerOptions);
        else
            return JsonSerializer.Deserialize<ThunderstorePackageExperimental>(text, SerializerOptions).Latest;
    }

    public string Namespace { get; set; }

    public string Name { get; set; }

    public PackageVersionExperimental Latest { get; set; }

    public IList<PackageListingExperimental> CommunityListings { get; } = [];

    public class PackageVersionExperimental : RoR2Plugin
    {
        public string DownloadUrl { get; set; }

        public ulong Downloads { get; set; }
    }

    public class PackageListingExperimental
    {
        public bool HasNsfwContent { get; set; }

        public IList<string> Categories { get; } = [];

        public string Community { get; set; }

        public string ReviewStatus { get; set; }
    }
}
