using System.IO.Compression;
using System.Text.Json;
using Semver;

namespace DelugeManager;

public static class PluginHandler
{
    private static readonly List<string> abnormalFileHandling = [
        "RiskofThunder-BepInEx_GUI"
    ];

    private static readonly List<string> resolved = [];

    public static async Task InstallPluginToProfile(string profilePath, string name, string author, string version, RoR2Version? gameVersion = null, bool root = true)
    {
        if(root) resolved.Clear();

        Directory.CreateDirectory(P(Program.Folder, "_data", "staging"));

        var package = await ThunderstoreApi.GetPackageAsync(name, author, gameVersion, version);
        var profile = Program.profiles[profilePath];

        if(root && version != package.VersionNumber)
        {
            if(version != null)
            {
                if(gameVersion != null)
                    Console.WriteLine($"Could not resolve version {version} for the specified game version {gameVersion.Value.Identifier}, using nearest version: {package.VersionNumber}");
                else
                    Console.WriteLine($"Could not resolve version {version}, using nearest version: {package.VersionNumber}");
            }
            version = package.VersionNumber;
        }

        string fileCombinedNameAndVersion = $"{author}-{name}-{version}";

        if(!root && resolved.Contains(fileCombinedNameAndVersion)) return;

        if(version == profile.Mods.FirstOrDefault(x => x.Namespace == author && x.Name == name)?.VersionNumber)
        {
            if(root)
                Console.WriteLine($"The package {fileCombinedNameAndVersion} is already installed");
            return;
        }

        string fullProfilePath = Path.Combine(Program.Folder, "profiles", profilePath);
        string bepInExPath = P(fullProfilePath, "BepInEx");
        string fileCombinedName = $"{author}-{name}";

        string tempPackagePath = P(Program.Folder, "_data", "staging", fileCombinedNameAndVersion);

        Directory.CreateDirectory(P(Program.CacheFolder, "downloads"));
        var cachefile = P(Program.CacheFolder, "downloads", fileCombinedNameAndVersion);

        if(!Program.ValidateCache(cachefile + ".cache"))
        {
            if(File.Exists(cachefile + ".zip"))
                File.Delete(cachefile + ".zip");
            if(File.Exists(cachefile + ".cache"))
                File.Delete(cachefile + ".cache");

            Console.WriteLine($"Downloading package {fileCombinedNameAndVersion} from {ThunderstoreApi.PackageProvider}...");

            using var client = new HttpClient();
            using var file = File.Open(cachefile + ".zip", FileMode.Create);
            await (await client.GetStreamAsync(package.DownloadUrl)).CopyToAsync(file);
            await file.FlushAsync();
            file.Close();

            await File.WriteAllTextAsync(cachefile + ".cache", $"{DateTime.UtcNow}:::24");
        }

        Console.WriteLine($"Installing {fileCombinedNameAndVersion}...");

        ZipFile.ExtractToDirectory(cachefile + ".zip", tempPackagePath);

        string profilePackagePath = P(fullProfilePath, "BepInEx", "plugins", fileCombinedName);
        string profilePatcherPath = P(fullProfilePath, "BepInEx", "patchers", fileCombinedName);
        string profileMonomodPath = P(fullProfilePath, "BepInEx", "monomod", fileCombinedName);
        string profileCorePath    = P(fullProfilePath, "BepInEx", "core", fileCombinedName);

        Directory.CreateDirectory(profilePackagePath);

        void copyFileTempToOut(string filename, bool condition = true)
        {
            if(!condition || !File.Exists(P(tempPackagePath, filename))) return;
            File.Copy(P(tempPackagePath, filename), P(profilePackagePath, filename), true);
        }

        void copyPathAndDelete(string pathFrom, string pathTo)
        {
            if(!Directory.Exists(P(tempPackagePath, pathFrom))) return;
            Directory.CreateDirectory(pathTo);
            Program.CopyFilesRecursively(P(tempPackagePath, pathFrom), pathTo);
            Directory.Delete(P(tempPackagePath, pathFrom), true);
        }

        if(!abnormalFileHandling.Contains(fileCombinedName))
        {
            copyFileTempToOut("manifest.json");
            copyFileTempToOut("icon.png");
            copyFileTempToOut("README.md");
            copyFileTempToOut("CHANGELOG.md");
        }

        // we delete these after copying so that the recursive search for dlls goes fine
        copyPathAndDelete("BepInEx", bepInExPath);
        copyPathAndDelete("plugins", profilePackagePath);
        copyPathAndDelete("patchers", profilePatcherPath);
        copyPathAndDelete("monomod", profileMonomodPath);
        copyPathAndDelete("core", profileCorePath);

        CopyFilesFlattenedWithFilePattern(tempPackagePath, profileMonomodPath, "*.mm.dll", delete: true);
        CopyFilesFlattenedWithFilePattern(tempPackagePath, profilePackagePath, "*.dll", delete: true);

        profile.Mods.Add(new() {
            Dependencies = [..package.Dependencies],
            Description = package.Description,
            Enabled = true,
            FullName = package.FullName,
            Name = package.Name,
            Namespace = author,
            VersionNumber = package.VersionNumber,
            WebsiteURL = package.WebsiteUrl
        });

        resolved.Add(fileCombinedNameAndVersion);

        await ResolveDependencies(profilePath, package);

        Console.WriteLine($"  {fileCombinedNameAndVersion} installed successfully!");

        Directory.Delete(tempPackagePath, true);

        if(root) resolved.Clear();
    }

    public static async Task ResolveDependencies(string profilePath, PackageVersion mod)
    {
        var profile = Program.profiles[profilePath];

        string item = Path.Combine(Program.Folder, "profiles", profilePath);

        profile.Mods.Clear();
        Directory.CreateDirectory(Path.Combine(item, "BepInEx", "plugins"));
        foreach(var fullPath in Directory.EnumerateDirectories(Path.Combine(item, "BepInEx", "plugins")))
        {
            if(!Directory.EnumerateFileSystemEntries(fullPath).Any())
                continue;

            string manifestPath = Path.Combine(fullPath, "manifest.json");
            if(File.Exists(manifestPath))
            {
                profile.Mods.Add(JsonSerializer.Deserialize<RoR2Plugin>(File.ReadAllText(manifestPath), ModProfile.SerializerOptions));
            }
        }

        foreach(var requiredMod in mod.Dependencies)
        {
            string[] split = requiredMod.Split("-");

            bool exists = requiredMod.StartsWith("bbepis-BepInExPack");
            foreach(var mod2 in profile.Mods)
            {
                if(mod2.FullName == requiredMod)
                {
                    exists = true;
                    break;
                }
            }
            if(!exists)
            {
                await InstallPluginToProfile(profilePath, split[1], split[0], split[2], RoR2Versions.Versions.GetByID(profile.GameVersion), false);
            }
        }

        await File.WriteAllTextAsync(Path.Combine(Program.Folder, "profiles", profilePath, "profile.json"), JsonSerializer.Serialize(profile, ModProfile.SerializerOptions));
    }

    private static string P(params string[] paths) => Path.Combine(paths);

    private static void CopyFilesFlattenedWithFilePattern(string sourcePath, string targetPath, string filePattern = "*.*", bool delete = false)
    {
        foreach(var file in Directory.GetFiles(sourcePath, filePattern, SearchOption.AllDirectories))
        {
            File.Copy(file, P(targetPath, file.Split(Path.DirectorySeparatorChar)[^1]), true);
            if(delete) File.Delete(file);
        }
    }
}
