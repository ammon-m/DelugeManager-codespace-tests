using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DelugeManager;

public static class RoR2Versions
{
	public static string GetVersionsJson()
	{
		// RoR2Version[] versions = [..Versions];
		return JsonSerializer.Serialize((Dictionary<string, RoR2Version>)Versions, RoR2Version.SerializerOptions);
	}

	public static RoR2Version Latest => Versions[^2];

	public static RoR2VersionList Versions { get; } =
	[
		new(
			"2019_03_28_EarlyAccess_Launch",
			4785037072112130807,
			new(2019, 03, 28),
			displayName: "Early Access Launch",
			major: true,
			nextMajorVersion: "2019_06_25_EarlyAccess_ContentUpdate_ScorchedAcres",
			bepInExVersion: "2.0.0"
		),
		new(
			"2019_04_03_EarlyAccess_Patch",	
			5182444948675866264,
			new(2019, 04, 03),
			nextMajorVersion: "2019_06_25_EarlyAccess_ContentUpdate_ScorchedAcres",
			bepInExVersion: "2.0.0"
		),
		new(
			"2019_04_16_EarlyAccess_Patch",	
			1280738030908307534,
			new(2019, 04, 16),
			nextMajorVersion: "2019_06_25_EarlyAccess_ContentUpdate_ScorchedAcres",
			bepInExVersion: "2.0.0"
		),
		new(
			"2019_05_21_EarlyAccess_Patch",	
			3793891161392450065,
			new(2019, 05, 21),
			nextMajorVersion: "2019_06_25_EarlyAccess_ContentUpdate_ScorchedAcres",
			bepInExVersion: "2.0.0"
		),

		new(
			"2019_06_25_EarlyAccess_ContentUpdate_ScorchedAcres",
			2112019357004186123,
			new(2019, 06, 25),
			displayName: "Scorched Acres",
			major: true,
			nextMajorVersion: "2019_09_17_EarlyAccess_ContentUpdate_Skills2_0",
			bepInExVersion: "2.0.0"
		),
		new(
			"2019_07_03_EarlyAccess_Patch",
			3670024595274968264,
			new(2019, 07, 03),
			nextMajorVersion: "2019_09_17_EarlyAccess_ContentUpdate_Skills2_0",
			bepInExVersion: "2.0.0"
		),

		new(
			"2019_09_17_EarlyAccess_ContentUpdate_Skills2_0",
			4255430772592120193,
			new(2019, 09, 17),
			displayName: "Skills 2.0",
			major: true,
			nextMajorVersion: "2019_12_17_EarlyAccess_ContentUpdate_HiddenRealms",
			bepInExVersion: "2.0.0"
		),
		new(
			"2019_10_10_EarlyAccess_Patch",
			7012808837121522032,
			new(2019, 10, 10),
			nextMajorVersion: "2019_12_17_EarlyAccess_ContentUpdate_HiddenRealms",
			bepInExVersion: "2.0.0"
		),

		new(
			"2019_12_17_EarlyAccess_ContentUpdate_HiddenRealms",
			5239878991551190606,
			new(2019, 12, 17),
			displayName: "Hidden Realms",
			major: true,
			nextMajorVersion: "2020_03_31_EarlyAccess_ContentUpdate_Artifacts",
			bepInExVersion: "3.0.0"
		),

		new(
			"2020_03_31_EarlyAccess_ContentUpdate_Artifacts",
			2472907969637403728,
			new(2020, 03, 31),
			displayName: "Artifacts 2.0",
			major: true,
			nextMajorVersion: "2020_08_11_EarlyAccess_ContentUpdate_5",
			bepInExVersion: "3.2.0"
		),
		new(
			"2020_04_21_EarlyAccess_Patch",
			6052191829405703267,
			new(2020, 04, 21),
			nextMajorVersion: "2020_08_11_EarlyAccess_ContentUpdate_5",
			bepInExVersion: "3.2.0"
		),

		// FULL RELEASE

		new(
			"2020_08_11_EarlyAccess_ContentUpdate_5",
			6571321385300192800,
			new(2020, 08, 11),
			displayName: "1.0 Launch",
			major: true,
			nextMajorVersion: "2021_03_25_Update_Anniversary",
			bepInExVersion: "5.3.1"
		),
		new(
            "2020_08_13_EarlyAccess_ContentUpdate_5_Hotfix",
            9204909284884878595,
            new(2020, 08, 13),
			nextMajorVersion: "2021_03_25_Update_Anniversary",
			bepInExVersion: "5.3.1"
		),
		new(
            "2020_09_01_Patch_1_0_1_1",
            5049246425996249487,
            new(2020, 09, 01),
			nextMajorVersion: "2021_03_25_Update_Anniversary",
			bepInExVersion: "5.3.1"
		),
		new(
            "2020_11_03_Patch_1_0_2_0",
            3160519164886166204,
            new(2020, 11, 03),
			nextMajorVersion: "2021_03_25_Update_Anniversary",
			bepInExVersion: "5.3.1"
		),
		new(
            "2020_12_15_Patch_1_0_3_1",
            8643078234309832101,
            new(2020, 12, 15),
            stable: true,
			nextMajorVersion: "2021_03_25_Update_Anniversary",
			bepInExVersion: "5.3.1"
		),

		new(
            "2021_03_25_Update_Anniversary",
            7255008819833157291,
            new(2021, 03, 25),
            displayName: "Anniversary Update",
            major: true,
			nextMajorVersion: "2022_03_01_DLC_01_SurvivorsOfTheVoid",
			bepInExVersion: "5.4.1801"
		),
		new(
            "2021_04_12_Patch_1_1_1_2",
            4163843425391060582,
            new(2021, 04, 12),
			nextMajorVersion: "2022_03_01_DLC_01_SurvivorsOfTheVoid",
			bepInExVersion: "5.4.1801"
		),
		new(
			"2021_04_20_Patch_1_1_1_4",
      		2934004482569727060,
      		new(2021, 04, 20),
      		displayName: "Anniversary Update Stable",
      		stable: true,
			nextMajorVersion: "2022_03_01_DLC_01_SurvivorsOfTheVoid",
			bepInExVersion: "5.4.1801",
			fixPluginTypesSerializationVersion: "1.0.1"
		),

		new(
            "2022_03_01_DLC_01_SurvivorsOfTheVoid",
            5430547693553236352,
            new(2022, 03, 01),
            displayName: "Survivors of the Void Update + DLC",
            major: true,
			nextMajorVersion: "2023_11_06_Patch_7_5",
			bepInExVersion: "5.4.2112",
			fixPluginTypesSerializationVersion: "1.0.2"
		),
		new(
            "2022_03_11_Patch_1_2_2_0",
            226983827800243462,
            new(2022, 03, 11),
			nextMajorVersion: "2023_11_06_Patch_7_5",
			bepInExVersion: "5.4.2112",
			fixPluginTypesSerializationVersion: "1.0.2"
		),
		new(
            "2022_04_19_Patch_1_2_3_0",
            4649272427595582012,
            new(2022, 04, 19),
			nextMajorVersion: "2023_11_06_Patch_7_5",
			bepInExVersion: "5.4.2112",
			fixPluginTypesSerializationVersion: "1.0.2"
		),
		new(
            "2022_05_26_Patch_1_2_4_0",
            8981465225844154625,
            new(2022, 05, 26),
			nextMajorVersion: "2023_11_06_Patch_7_5",
			bepInExVersion: "5.4.2112",
			fixPluginTypesSerializationVersion: "1.0.2"
		),
		new(
            "2022_09_29_Patch_1_2_4_1",
            7660073450841700654,
            new(2022, 09, 29),
            displayName: "SOTV Stable",
            stable: true,
			nextMajorVersion: "2023_11_06_Patch_7_5",
			bepInExVersion: "5.4.2112",
			fixPluginTypesSerializationVersion: "1.0.2"
		),

		// GEARBOX :pensive:

		new(
            "2023_11_06_Patch_7_5",
            2538203695974683966,
            new(2023, 11, 06),
            displayName: "(unnamed) \"Comet\" Update",
            stable: true,
			nextMajorVersion: "2024_05_20_Update_Devotion",
			bepInExVersion: "5.4.2113",
			ror2BepInExPackVersion: "1.11.0",
			fixPluginTypesSerializationVersion: "1.0.3"
		),

		new(
            "2024_05_20_Update_Devotion",
            9058106608706845920,
            new(2024, 05, 20),
            displayName: "Devotion Update Stable",
            major: true,
            stable: true,
			nextMajorVersion: "2024_08_27_DLC_02_SeekersOfTheStorm",
			bepInExVersion: "5.4.2115",
			ror2BepInExPackVersion: "1.16.0",
			fixPluginTypesSerializationVersion: "1.0.4"
		),

		new(
            "2024_08_27_DLC_02_SeekersOfTheStorm",
            4567638355138669926,
            new(2024, 08, 27),
            displayName: "Seekers of the Storm Update + DLC",
            major: true,
			nextMajorVersion: "Latest",
			bepInExVersion: "5.4.2117",
			ror2BepInExPackVersion: "1.20.0",
			fixPluginTypesSerializationVersion: "1.0.4"
		),

		new(
            "Latest",
            -1,
            null
		)
	];
}

public class RoR2VersionList : IEnumerable<RoR2Version>, ICollection<RoR2Version>
{
	private Dictionary<string, RoR2Version> keyValues = [];

	public int Count => keyValues.Count;
	public bool IsReadOnly => false;

	public RoR2Version this[int index]
	{
		get {
			int i = 0;
			foreach(var item in this)
			{
				if(i == index) return item;
				i++;
			}

			throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	public static explicit operator Dictionary<string, RoR2Version>(RoR2VersionList list)
	{
		return list.keyValues.ToDictionary();
	}

	public static RoR2VersionList From(IEnumerable<KeyValuePair<string, RoR2Version>> dict)
	{
		RoR2VersionList list = [];
		foreach(var pair in dict)
		{
			var item = pair.Value;
			item.Identifier = pair.Key;

			list.keyValues.Add(pair.Key, item);
		}
		return list;
	}

	public void SetFrom(IEnumerable<KeyValuePair<string, RoR2Version>> dict)
	{
		this.keyValues = From(dict).keyValues;
	}

	public void Add(RoR2Version item)
	{
		keyValues.Add(item.Identifier, item);
	}

	public void Clear()
	{
		keyValues.Clear();
	}

	public bool Contains(RoR2Version item)
	{
		return keyValues.ContainsValue(item) || keyValues.ContainsKey(item.Identifier);
	}

	public bool Contains(string identifier)
	{
		return keyValues.ContainsKey(identifier);
	}

	public RoR2Version GetByID(string identifier)
	{
		return keyValues[identifier];
	}

	public bool TryGetByID(string identifier, out RoR2Version value)
	{
		return keyValues.TryGetValue(identifier, out value);
	}

	public void CopyTo(RoR2Version[] array, int arrayIndex)
	{
		keyValues.Values.CopyTo(array, arrayIndex);
	}

	public IEnumerator<RoR2Version> GetEnumerator()
	{
		return keyValues.Values.GetEnumerator();
	}

	public bool Remove(RoR2Version item)
	{
		return keyValues.Remove(item.Identifier);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		yield return GetEnumerator();
	}
}

public struct RoR2Version(
    string identifier,
    long manifest,
    DateOnly? date,
	string nextMajorVersion = null,
    string displayName = null,
    bool major = false,
    bool stable = false,
	string bepInExVersion = null,
	string ror2BepInExPackVersion = null,
	string fixPluginTypesSerializationVersion = null
)
{
	public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

	public string Identifier { get; set; } = identifier ?? "unknown";

	public string NextMajorVersion { get; set; } = nextMajorVersion;

	public long Manifest { get; set; } = manifest;

	public DateOnly? Date { get; set; } = date;

	public string DisplayName { get; set; } = displayName;

	public bool Major { get; set; } = major;

	public bool Stable { get; set; } = stable;

	[JsonPropertyName("BepInEx_version")]
	public string BepInExVersion { get; set; } = bepInExVersion;

	[JsonPropertyName("RoR2BepInExPack_version")]
	public string RoR2BepInExPackVersion { get; set; } = ror2BepInExPackVersion;

	[JsonPropertyName("FixPluginTypesSerialization_version")]
	public string FixPluginTypesSerializationVersion { get; set; } = fixPluginTypesSerializationVersion;
}
