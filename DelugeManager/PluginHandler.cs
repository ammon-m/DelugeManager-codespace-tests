using System.IO.Compression;

namespace DelugeManager;

public static class PluginHandler
{
    public static async Task InstallPluginToProfile(string profilePath, string name, string author, string version)
    {
        string bepInExPath = P(profilePath, "BepInEx");
        string fileCombinedName = $"{author}-{name}";

        string tempPackagePath = P(Program.Folder, "_data", "staging", fileCombinedName.GetHashCode().ToString());
        Directory.CreateDirectory(tempPackagePath);

        Console.WriteLine($"Pulling {author}-{name} package from Thunderstore.io...");
        var apiResponse = await ThunderstorePackageExperimental.GetPackageAsync(name, author, version);

        using Stream ms = new MemoryStream();

        var cachefile = P(Program.CacheFolder, fileCombinedName.GetHashCode().ToString());
        bool cacheAvailable = false;
        if(Program.ValidateCache(cachefile + ".cache"))
        {
            cacheAvailable = true;
            using var s = File.OpenRead(cachefile + ".zip");
            await s.CopyToAsync(ms);
        }
        else
        {
            File.Delete(cachefile + ".zip");
            File.Delete(cachefile + ".cache");
        }

        if(!cacheAvailable)
        {
            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(apiResponse.DownloadUrl);
            await s.CopyToAsync(ms);
            await s.CopyToAsync(new FileStream(cachefile + ".zip", FileMode.Create));

            File.WriteAllText(cachefile + ".cache", $"{DateTime.UtcNow}:::24");
        }

        ZipFile.ExtractToDirectory(ms, tempPackagePath);

        Console.WriteLine($"Installing {author}-{name}...");

        string profilePackagePath = P(profilePath, "BepInEx", "plugins", fileCombinedName);
        string profilePatcherPath = P(profilePath, "BepInEx", "patchers", fileCombinedName);
        string profileMonomodPath = P(profilePath, "BepInEx", "monomod", fileCombinedName);
        string profileCorePath    = P(profilePath, "BepInEx", "core", fileCombinedName);

        void copyFileTempToOut(string filename, bool condition = true)
        {
            if(!condition) return;
            File.Copy(P(tempPackagePath, filename), P(profilePackagePath, filename), true);
        }

        void copyPathAndDelete(string pathFrom, string pathTo)
        {
            if(!Directory.Exists(P(tempPackagePath, pathFrom))) return;
            Directory.CreateDirectory(pathTo);
            Program.CopyFilesRecursively(P(tempPackagePath, pathFrom), pathTo);
            Directory.Delete(P(tempPackagePath, pathFrom), true);
        }

        copyFileTempToOut("manifest.json");
        copyFileTempToOut("icon.png");
        copyFileTempToOut("README.md");
        copyFileTempToOut("CHANGELOG.md", File.Exists(P(tempPackagePath, "CHANGELOG.md")));

        // we delete these after copying so that the recursive search for dlls goes fine
        copyPathAndDelete("BepInEx", bepInExPath);
        copyPathAndDelete("plugins", profilePackagePath);
        copyPathAndDelete("patchers", profilePatcherPath);
        copyPathAndDelete("monomod", profileMonomodPath);
        copyPathAndDelete("core", profileCorePath);

        CopyFilesFlattenedWithFilePattern(tempPackagePath, profileMonomodPath, "*.mm.dll", true);
        CopyFilesFlattenedWithFilePattern(tempPackagePath, profilePackagePath, "*.dll", true);

        Console.WriteLine($"{author}-{name} installed successfully!");
    }

    private static string P(params string[] paths) => Path.Combine(paths);

    static void CopyFilesFlattenedWithFilePattern(string sourcePath, string targetPath, string filePattern = "*.*", bool delete = false)
    {
        foreach(var file in Directory.GetFiles(sourcePath, filePattern, SearchOption.AllDirectories))
        {
            File.Copy(file, P(targetPath, file.Split(Path.DirectorySeparatorChar)[^1]), true);
            if(delete) File.Delete(file);
        }
    }
}
