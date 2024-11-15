using System.Text.Json.Serialization;

namespace DelugeManager;

public class PackageIndex
{
    public Package[] Packages { get; set; } = [];
}

public class Package
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("full_name")]
	public string FullName { get; set; }

	[JsonPropertyName("owner")]
	public string Owner { get; set; }

	[JsonPropertyName("package_url")]
	public string PackageUrl { get; set; }

	[JsonPropertyName("donation_link")]
	public string DonationLink { get; set; }

	[JsonPropertyName("date_created")]
	public string DateCreated { get; set; }

	[JsonPropertyName("date_updated")]
	public string DateUpdated { get; set; }

	[JsonPropertyName("uuid4")]
	public string Uuid4 { get; set; }

	[JsonPropertyName("rating_score")]
	public int RatingScore { get; set; }

	[JsonPropertyName("is_pinned")]
	public bool IsPinned { get; set; }

	[JsonPropertyName("is_deprecated")]
	public bool IsDeprecated { get; set; }

	[JsonPropertyName("has_nsfw_content")]
	public bool HasNsfwContent { get; set; }

	[JsonPropertyName("categories")]
	public string[] Categories { get; set; }

	[JsonPropertyName("versions")]
	public PackageVersion[] Versions { get; set; }
}

public class PackageVersion
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("full_name")]
	public string FullName { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("icon")]
	public Uri Icon { get; set; }

	[JsonPropertyName("version_number")]
	public string VersionNumber { get; set; }

	[JsonPropertyName("dependencies")]
	public string[] Dependencies { get; set; }

	[JsonPropertyName("download_url")]
	public Uri DownloadUrl { get; set; }

	[JsonPropertyName("downloads")]
	public long Downloads { get; set; }

	[JsonPropertyName("date_created")]
	public DateTimeOffset DateCreated { get; set; }

	[JsonPropertyName("website_url")]
	public Uri WebsiteUrl { get; set; }

	[JsonPropertyName("is_active")]
	public bool IsActive { get; set; }

	[JsonPropertyName("uuid4")]
	public Guid Uuid4 { get; set; }

	[JsonPropertyName("file_size")]
	public long FileSize { get; set; }
}
