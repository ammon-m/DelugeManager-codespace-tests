using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

using DelugeManager;

internal class Program
{
    static bool exiting = false;
    static int exitCode = 0;

    static bool interactiveMode = false;

    static readonly Dictionary<string, RoR2Profile> profiles = [];

    static readonly List<string> instances = [];

    const string DDVersion = "2.7.1";

    public static string Folder => Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

    public static string CacheFolder {
        get {
            var path = Path.Combine(Folder, "_data", "cache");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public const System.Globalization.DateTimeStyles cacheExpiryDateTimeStyles =
        System.Globalization.DateTimeStyles.AllowLeadingWhite &
        System.Globalization.DateTimeStyles.AllowTrailingWhite &
        System.Globalization.DateTimeStyles.AssumeUniversal &
        System.Globalization.DateTimeStyles.AdjustToUniversal;

    static bool profilesDirty = true;

    internal static string os;
    internal static Architecture arch;

    internal const string osWindows = "windows";
    internal const string osLinux = "linux";

    static string depotDownloaderProgram;
    static string username;

    static async Task<int> Main(string[] args)
    {
        arch = RuntimeInformation.OSArchitecture;

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            os = osWindows;
            depotDownloaderProgram = Path.Combine(Folder, "_data", "DepotDownloader", "DepotDownloader.exe");
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            os = osLinux;
            depotDownloaderProgram = Path.Combine(Folder, "_data", "DepotDownloader", "DepotDownloader");
        }
        else
        {
            Console.WriteLine($"This program is broken on the operating system {os}");
            return 1;
        }

        Directory.CreateDirectory(Path.Combine(Folder, "_data"));
        Directory.CreateDirectory(Path.Combine(Folder, "_data", "staging"));
        Directory.CreateDirectory(Path.Combine(Folder, "profiles"));
        Directory.CreateDirectory(Path.Combine(Folder, "instances"));

        var rootCommand = new RootCommand(
            description: "Sample app for System.CommandLine"
        );

        if(args.Length > 0 && (args[0] == "--interactiveMode" || args[0] == "-i"))
        {
            args = args[1..];
            interactiveMode = true;
        }

        // list command
        {
            var listCommand = new Command(
                name: "list",
                description: "Prints one of several lists"
            );

            // versions
            {
                var versionsCommand = new Command(
                    name: "versions",
                    description: "Lists all installable versions"
                );

                var showManifestsOption = new Option<bool>(
                    aliases: ["--showManifests", "-m"],
                    description: "Show manifest ids",
                    getDefaultValue: () => false
                )
                {
                    IsRequired = false
                };
                versionsCommand.AddOption(showManifestsOption);

                var showDatesOption = new Option<bool>(
                    aliases: ["--showDates", "-d"],
                    description: "Show release dates",
                    getDefaultValue: () => false
                )
                {
                    IsRequired = false
                };
                versionsCommand.AddOption(showDatesOption);

                var showDisplayNamesOption = new Option<bool>(
                    aliases: ["--showDisplayNames", "-n"],
                    description: "Show display names, if present",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                versionsCommand.AddOption(showDisplayNamesOption);

                var noBullshitOption = new Option<bool>(
                    aliases: ["--noBullshit", "-b"],
                    description: "Hide all decorative information and make output YAML serializable",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                versionsCommand.AddOption(noBullshitOption);

                var rawInfoOption = new Option<bool>(
                    aliases: ["--rawInfo", "-r"],
                    description: "Print the entire list as a JSON object (ignores all other options)",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                versionsCommand.AddOption(rawInfoOption);

                versionsCommand.SetHandler(
                    (showManifests, showDates, showDisplayNames, noBullshit, rawInfo) => {
                        Console.ResetColor();

                        if(rawInfo)
                        {
                            Console.WriteLine(RoR2Versions.GetVersionsJson());
                            return;
                        }

                        if(noBullshit)
                            Console.Write("Versions:");
                        else
                            Console.Write("Available Risk of Rain 2 versions:");

                        RoR2Version lastVersion = default;
                        foreach(var version in RoR2Versions.Versions)
                        {
                            Console.WriteLine();
                            Console.Write("- Identifier: ");

                            if(version.Major) Console.ForegroundColor = ConsoleColor.Yellow;

                            Console.Write($"{version.Identifier}");

                            Console.ResetColor();

                            if(version.Stable && !noBullshit)
                            {
                                Console.Write(" - ");

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("STABLE");

                                Console.ResetColor();
                            }

                            Console.WriteLine();

                            if(showDisplayNames)
                            {
                                if(version.DisplayName is not null)
                                {
                                    Console.WriteLine($"  Name: {version.DisplayName}");
                                }
                                else if(version.Manifest == -1 && lastVersion.DisplayName is not null && !noBullshit)
                                {
                                    Console.WriteLine($"  Name: {lastVersion.DisplayName}");
                                }
                            }

                            if(showManifests)
                            {
                                Console.Write($"  Manifest: {(version.Manifest == -1 ? $"-1" : version.Manifest)}");
                                if(version.Manifest == -1 && !noBullshit)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGray;
                                    Console.Write($" ({lastVersion.Manifest})");
                                    Console.ResetColor();
                                }
                                Console.WriteLine();
                            }

                            if(showDates && version.Date is not null)
                            {
                                Console.WriteLine($"  Released: {version.Date}");
                            }

                            lastVersion = version;
                        }

                        Console.Write("Versions with ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("yellow");
                        Console.ResetColor();
                        Console.WriteLine(" identifiers are major updates.");

                        Console.Write("Versions marked as ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("STABLE");
                        Console.ResetColor();
                        Console.WriteLine(" are common versions for mods to use.");

                        Console.WriteLine("Use the Identifier (shown in YYYY_MM_DD_VersionName format) for downloading");
                    },
                    showManifestsOption, showDatesOption, showDisplayNamesOption, noBullshitOption, rawInfoOption
                );

                listCommand.AddCommand(versionsCommand);
            }

            // profiles
            {
                var profilesCommand = new Command(
                    name: "profiles",
                    description: "Lists all profiles"
                );

                var showDisplayNamesOption = new Option<bool>(
                    aliases: ["--showDisplayNames", "-n"],
                    description: "Show the display name next to each identifier, if present",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                profilesCommand.AddOption(showDisplayNamesOption);

                var showFullPathsOption = new Option<bool>(
                    aliases: ["--showFullPaths", "-p"],
                    description: "Show full filepaths",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                profilesCommand.AddOption(showFullPathsOption);

                profilesCommand.SetHandler(
                    (showDisplayNames, showFullPaths) => {
                        Console.ResetColor();
                        CheckProfilesDirty();

                        if(profiles.Count == 0)
                        {
                            Console.WriteLine("You have no profiles installed.");
                            return;
                        }

                        Console.Write("Currently installed profiles:");

                        foreach(var item in profiles)
                        {
                            Console.WriteLine();
                            Console.Write("  ");

                            if(showFullPaths)
                                Console.Write(Path.Combine(Folder, "profiles", item.Key));
                            else
                                Console.Write(item.Key);

                            if(item.Key != item.Value.Name && showDisplayNames)
                                Console.Write($" ({item.Value.Name})");
                        }
                    },
                    showDisplayNamesOption, showFullPathsOption
                );

                listCommand.AddCommand(profilesCommand);
            }

            // instances
            {
                var instancesCommand = new Command(
                    name: "installed-versions",
                    description: "Lists currently installed versions of Risk of Rain 2"
                );

                var showDisplayNamesOption = new Option<bool>(
                    aliases: ["--showDisplayNames", "-n"],
                    description: "Show the display name next to each identifier, if present",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                instancesCommand.AddOption(showDisplayNamesOption);

                var showFullPathsOption = new Option<bool>(
                    aliases: ["--showFullPaths", "-p"],
                    description: "Show full filepaths",
                    getDefaultValue: () => false
                ) {
                    IsRequired = false
                };
                instancesCommand.AddOption(showFullPathsOption);

                instancesCommand.SetHandler(
                    (showDisplayNames, showFullPaths) => {
                        Console.ResetColor();
                        CheckProfilesDirty();

                        if(instances.Count == 0)
                        {
                            Console.WriteLine("You have no instances installed.");
                            return;
                        }

                        Console.Write("Currently installed instances:");

                        foreach(var item in instances)
                        {
                            Console.WriteLine();
                            Console.Write("  ");

                            var v = RoR2Versions.Versions.GetByID(item);

                            if(showFullPaths)
                                Console.Write(Path.Combine(Folder, "instances", item));
                            else
                                Console.Write(item);

                            if(v.DisplayName is not null && showDisplayNames)
                                Console.Write($" ({v.DisplayName})");
                        }
                    },
                    showDisplayNamesOption, showFullPathsOption
                );

                listCommand.AddCommand(instancesCommand);
            }

            rootCommand.AddCommand(listCommand);
        }

        // exit command
        if(interactiveMode)
        {
            var exitCommand = new Command(
                name: "exit",
                description: "Closes the program"
            );

            exitCommand.SetHandler(() => {
                exiting = true;
                exitCode = 0;
            });
            rootCommand.AddCommand(exitCommand);
        }

        // interactiveModeOption
        if(!interactiveMode)
        {
            var interactiveModeOption = new Option<bool>(
                aliases: ["--interactiveMode", "-i"],
                description: "Enables continous CLI use of the program in a loop. Use \"exit\" to exit the loop",
                getDefaultValue: () => false
            );

            rootCommand.AddOption(interactiveModeOption);
        }

        // profile command
        {
            var profileCommand = new Command(name: "profile", description: "Manage launcher profiles");

            // new
            {
                var newCommand = new Command(name: "new", "Creates a new profile");

                var forceOverwriteOption = new Option<bool>(
                    aliases: ["--forceOverwrite", "-f"],
                    getDefaultValue: () => false,
                    description: "If the folder already exists, overwrite it without question.")
                {
                    IsRequired = false,
                };
                newCommand.AddOption(forceOverwriteOption);

                var launchArgumentsOption = new Option<string>(
                    aliases: ["--launchArguments", "-l"],
                    getDefaultValue: () => "\"\"",
                    description: "Pre-configure the launch arguments that the profile will use with the \"profile launch\" command")
                {
                    IsRequired = false,
                };
                newCommand.AddOption(launchArgumentsOption);

                var nameArgument = new Argument<string>("name", "Name of the profile");
                newCommand.AddArgument(nameArgument);

                var targetVersionArgument = new Argument<string>("targetVersion", "Risk of Rain 2 version that the profile is for");
                newCommand.AddArgument(targetVersionArgument);

                newCommand.SetHandler(
                    async (forceOverwrite, launchArguments, name, targetVersion) => {
                        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
                        ArgumentException.ThrowIfNullOrWhiteSpace(targetVersion, nameof(targetVersion));

                        if(
                            targetVersion.Equals("anniversary", StringComparison.OrdinalIgnoreCase) ||
                            targetVersion.Equals("anniversary update", StringComparison.OrdinalIgnoreCase)
                        ) {
                            targetVersion = "2021_04_20_Patch_1_1_1_4";
                        }

                        if(
                            targetVersion.Equals("sotv", StringComparison.OrdinalIgnoreCase) ||
                            targetVersion.Equals("survivors of the void", StringComparison.OrdinalIgnoreCase)
                        ) {
                            targetVersion = "2022_09_29_Patch_1_2_4_1";
                        }

                        if(
                            targetVersion.Equals("devotion", StringComparison.OrdinalIgnoreCase) ||
                            targetVersion.Equals("devotion update", StringComparison.OrdinalIgnoreCase)
                        ) {
                            targetVersion = "2024_05_20_Update_Devotion";
                        }

                        profilesDirty = true;
                        CheckProfilesDirty();

                        var path = name;
                        foreach(char c in Path.GetInvalidPathChars())
                        {
                            path = path.Replace(c, '_');
                        }

                        if(profiles.ContainsKey(path))
                        {
                            int i = 1;
                            while(profiles.ContainsKey($"{path} ({i})"))
                                i++;
                            path += $" ({i})";
                        }

                        if(!RoR2Versions.Versions.Contains(targetVersion))
                        {
                            Console.WriteLine("Invalid version target!, use \"list versions\" to see available versions, or press enter to view now");
                            if(Console.ReadKey().Key == ConsoleKey.Enter)
                            {
                                await rootCommand.InvokeAsync("list versions -n");
                            }
                            return;
                        }

                        var dir = Path.Combine(Folder, "profiles", path);

                        var profile = new RoR2Profile {
                            Name = name,
                            GameVersion = targetVersion,
                            LaunchArguments = launchArguments,
                        };

                        if(Directory.Exists(dir))
                        {
                            Console.Write($"A folder already exists with the name \"{path}\" ({dir})");
                            if(forceOverwrite)
                            {
                                Console.WriteLine(", overwriting..");
                                Directory.Delete(dir, true);
                                Directory.CreateDirectory(dir);
                            }
                            else
                            {
                                Console.WriteLine();
                                if(ConsoleUtils.Confirm("Would you like to overwrite it and delete all contained files? (answering with \"n\" will merge the new profile into the folder without removing anything. Beware of bugs!)"))
                                {
                                    Directory.Delete(dir, true);
                                    Directory.CreateDirectory(dir);
                                }
                            }
                        }

                        Directory.CreateDirectory(dir);
                        File.WriteAllText(Path.Combine(dir, "profile.json"), JsonSerializer.Serialize(profile, RoR2Profile.SerializerOptions));

                        profiles.Add(path, profile);

                        profilesDirty = true;

                        if(!instances.Contains(targetVersion))
                        {
                            Console.WriteLine("The selected version has not been installed and will be acquired via DepotDownloader");

                            bool result = await GetGameVersion(targetVersion);

                            Console.WriteLine();
                        }

                        await AddCoreModsToProfile(dir, profile);

                        AddShortcutToProfile(path);

                        Console.WriteLine($"Created profile {name} ({dir})");
                    },
                    forceOverwriteOption, launchArgumentsOption, nameArgument, targetVersionArgument
                );

                profileCommand.AddCommand(newCommand);
            }

            // launch
            {
                var launchCommand = new Command(name: "launch", description: "Launch the game with the specified profile");

                var profileArg = new Argument<string>(name: "profile folder");
                launchCommand.AddArgument(profileArg);

                launchCommand.SetHandler(
                    async (_profile) => {
                        profilesDirty = true;
                        CheckProfilesDirty();

                        if(profiles.TryGetValue(_profile, out RoR2Profile profile))
                        {
                            if(!instances.Contains(profile.GameVersion))
                            {
                                Console.WriteLine("The profile's target version has not been installed and will be acquired via DepotDownloader");

                                if(!await GetGameVersion(profile.GameVersion))
                                {
                                    Console.WriteLine();
                                    return;
                                }
                            }

                            AddShortcutToProfile(_profile);

                            await VerifyBepInExPackInstalled(profile.GameVersion);

                            Console.WriteLine();
                            Console.WriteLine("Attempting to launch game...");

                            string arguments = GetProfileLaunchArguments(_profile);

                            var process = new Process {
                                StartInfo = new ProcessStartInfo {
                                    FileName = Path.Combine(Folder, "instances", profile.GameVersion, "Risk of Rain 2" + (os == osWindows ? ".exe" : "")),
                                    Arguments = arguments
                                }
                            };

                            if(process.Start())
                            {
                                Console.WriteLine($"Launching instance {profile.GameVersion} with arguments: {arguments}");
                                Console.WriteLine("Risk of Rain 2 will now open shortly!");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"The specified profile folder \"{_profile}\" does not exist");
                            return;
                        }
                    },
                    profileArg
                );

                profileCommand.AddCommand(launchCommand);
            }

            rootCommand.AddCommand(profileCommand);
        }

        if(args.Length > 0)
            return await rootCommand.InvokeAsync(args);
        else if(!interactiveMode)
            return await rootCommand.InvokeAsync("-h");
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("DelugeManager ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[v1.0.0]\n");

            Console.ResetColor();

            Console.WriteLine();

            await rootCommand.InvokeAsync("-h");

            // CLI loop
            while(!exiting)
            {
                Console.Write("> ");
                List<string> command = [..Console.ReadLine().Split(' ')];

                if(command.Remove("--interactiveMode") || command.Remove("-i"))
                {
                    command.Add("");
                }

                if(command[0] == "help") command[0] = "-h";
                if(command[0] == "version") command[0] = "--version";

                if(command.Count > 0 && !(command.Count == 1 && command[0].Length == 0))
                    await rootCommand.InvokeAsync([..command]);

                if(exiting) break;

                if(command.Count > 0 && !(command.Count == 1 && command[0].Length == 0))
                    Console.WriteLine();
            }

            return exitCode;
        }
    }

    static string GetProfileLaunchArguments(string profileName, bool relative = false)
    {
        return
        $"--doorstop-enabled true " +
        (relative
            ? $"--doorstop-target-assembly \"{Path.Combine(Folder, "profiles", profileName, "BepInEx", "core", "BepInEx.Preloader.dll")}\" "
            : $"--doorstop-target-assembly \"{Path.Combine(".", "BepInEx", "core", "BepInEx.Preloader.dll")}\" ") +
        $"{profiles[profileName].LaunchArguments ?? ""} ";
    }

    private static async Task<bool> GetGameVersion(string targetVersion)
    {
        await GetDepotDownloader();

        var v = RoR2Versions.Versions.GetByID(targetVersion);

        Console.WriteLine();
        Console.WriteLine("You need to log in with Steam credentials to download the game from Steam servers. Your password will not be saved");
        Console.WriteLine("Expect Steam Guard to trigger during the download process");
        Console.WriteLine();

        if(username is null)
        {
            Console.Write("Steam login username: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            username = Console.ReadLine();
            Console.ResetColor();
        }

        Console.WriteLine("Beginning download, this may take several minutes");
        Console.WriteLine("The process will start in a new window");

        var process = new Process {
            StartInfo = new() {
                FileName = depotDownloaderProgram,
                Arguments = $"-app 632360 " +
                            $"-depot 632361 " +
                            (v.Manifest == -1 ? "" : $"-manifest {v.Manifest} ") +
                            $"-username {username} " +
                            $"-dir \"{Path.Combine(Folder, "instances", v.Identifier)}\" ",
                UseShellExecute = true,
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if(process.ExitCode == 0)
        {
            Console.WriteLine($"Downloaded version {targetVersion} successfully!");

            await InstallBepInExPackForInstance(targetVersion, v.BepInExVersion);

            return true;
        }
        else
        {
            Console.WriteLine("Something went wrong while downloading");
            Console.WriteLine("DepotDownloader exit code: " + process.ExitCode);
            return false;
        }
    }

    public static bool ValidateCache(string cachefile)
    {
        if(!File.Exists(cachefile)) return false;

        var cacheInfo = File.ReadAllText(cachefile).Split(":::")[..2];

        var cacheDate = DateTime.Parse(cacheInfo[0], null, cacheExpiryDateTimeStyles);

        return (DateTime.UtcNow - cacheDate).TotalHours <= uint.Parse(cacheInfo[1]);
    }

    static async Task VerifyBepInExPackInstalled(string gameVersion)
    {
        var instancePath = Path.Combine(Folder, "instances", gameVersion);
        if(
            !Directory.Exists(Path.Combine(instancePath, "BepInEx")) ||
            !File.Exists(Path.Combine(instancePath, "winhttp.dll")) ||
            !File.Exists(Path.Combine(instancePath, ".doorstop_version")) ||
            !File.Exists(Path.Combine(instancePath, "doorstop_config.ini"))
        ) {
            await InstallBepInExPackForInstance(gameVersion, RoR2Versions.Versions.GetByID(gameVersion).BepInExVersion);
        }
    }

    static async Task InstallBepInExPackForInstance(string targetGameVersion, string targetBepInExVersion)
    {
        Console.WriteLine($"Pulling BepInExPack Package from Thunderstore.io...");

        {
            var apiResponse = await ThunderstorePackageExperimental.GetPackageAsync("BepInExPack", "bbepis", targetBepInExVersion);

            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(apiResponse.DownloadUrl);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);

            Console.WriteLine($"Installing BepInExPack...");

            string outputPackageFolder = Path.Combine(Folder, "_data", "staging", targetBepInExVersion.GetHashCode().ToString());
            Directory.CreateDirectory(outputPackageFolder);

            ZipFile.ExtractToDirectory(ms, outputPackageFolder);
            CopyFilesRecursively(Path.Combine(outputPackageFolder, "BepInExPack"), Path.Combine(Folder, "instances", targetGameVersion));

            Directory.Delete(outputPackageFolder, true);
        }

        Console.WriteLine($"BepInExPack installed successfully!");
    }

    static async Task AddCoreModsToProfile(string profilePath, RoR2Profile profile)
    {
        var gameVersion = RoR2Versions.Versions.GetByID(profile.GameVersion);
        var targetBepInExVersion = gameVersion.BepInExVersion;

        {
            Console.WriteLine($"Pulling BepInExPack Package from Thunderstore.io...");
            var apiResponse = await ThunderstorePackageExperimental.GetPackageAsync("BepInExPack", "bbepis", targetBepInExVersion);

            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(apiResponse.DownloadUrl);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);

            Console.WriteLine($"Installing BepInExPack...");

            string outputPackageFolder = Path.Combine(Folder, "_data", "staging", targetBepInExVersion.GetHashCode().ToString());
            Directory.CreateDirectory(outputPackageFolder);

            ZipFile.ExtractToDirectory(ms, outputPackageFolder);
            CopyFilesRecursively(Path.Combine(outputPackageFolder, "BepInExPack"), profilePath);

            Directory.Delete(outputPackageFolder, true);
            Console.WriteLine($"BepInExPack installed successfully!");
        }

        // for versions that require FixPluginTypesSerialization
        if(gameVersion.FixPluginTypesSerializationVersion is not null)
        {
            Console.WriteLine($"Pulling RiskofThunder-FixPluginTypesSerialization Package from Thunderstore.io...");
            var apiResponse = await ThunderstorePackageExperimental.GetPackageAsync("FixPluginTypesSerialization", "RiskofThunder", gameVersion.FixPluginTypesSerializationVersion);

            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(apiResponse.DownloadUrl);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);

            Console.WriteLine($"Installing RiskofThunder-FixPluginTypesSerialization...");

            string outputPackageFolder = Path.Combine(Folder, "_data", "staging", gameVersion.FixPluginTypesSerializationVersion.GetHashCode().ToString());
            Directory.CreateDirectory(outputPackageFolder);

            ZipFile.ExtractToDirectory(ms, outputPackageFolder);
            CopyFilesRecursively(Path.Combine(outputPackageFolder, "BepInEx"), Path.Combine(profilePath, "BepInEx"));

            Directory.Delete(outputPackageFolder, true);
            Console.WriteLine($"RiskofThunder-FixPluginTypesSerialization installed successfully!");
        }

        // for versions that require RoR2BepInExPack
        if(gameVersion.RoR2BepInExPackVersion is not null)
        {
            Console.WriteLine($"Pulling RiskofThunder-RoR2BepInExPack Package from Thunderstore.io...");
            var apiResponse = await ThunderstorePackageExperimental.GetPackageAsync("RoR2BepInExPack", "RiskofThunder", gameVersion.RoR2BepInExPackVersion);

            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(apiResponse.DownloadUrl);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);

            Console.WriteLine($"Installing RiskofThunder-RoR2BepInExPack...");

            string outputPackageFolder = Path.Combine(Folder, "_data", "staging", gameVersion.RoR2BepInExPackVersion.GetHashCode().ToString());
            Directory.CreateDirectory(outputPackageFolder);

            ZipFile.ExtractToDirectory(ms, outputPackageFolder);
            CopyFilesRecursively(Path.Combine(outputPackageFolder, "BepInEx"), Path.Combine(profilePath, "BepInEx"));

            Console.WriteLine($"RiskofThunder-RoR2BepInExPack installed successfully!");
        }
    }

    internal static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        foreach(var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach(var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    static async Task GetDepotDownloader()
    {
        var depotDownloaderPath = Path.Combine(Folder, "_data", "DepotDownloader");
        if(!Directory.Exists(depotDownloaderPath))
        {
            Console.WriteLine("Downloading and extracting DepotDownloader from github...");

            if(
                arch == Architecture.X86 ||
                arch == Architecture.Wasm ||
                arch == Architecture.S390x ||
                arch == Architecture.LoongArch64 ||
                arch == Architecture.Armv6)
            {
                Console.WriteLine("Unable to acquire DepotDownloader: there is no version that supports the architecture " + arch);
                Console.WriteLine("im really sorry for you :<");
                return;
            }

            if(os == osWindows)
            {
                if(arch != Architecture.X64 && arch != Architecture.Arm64)
                {
                    Console.WriteLine($"Unable to acquire DepotDownloader: there is no version that supports the architecture {arch} on the operating system {os}");
                    return;
                }
            }
            else if(os == osLinux)
            {
                if(arch != Architecture.X64 && arch != Architecture.Arm64 && arch != Architecture.Arm)
                {
                    Console.WriteLine($"Unable to acquire DepotDownloader: there is no version that supports the architecture {arch} on the operating system {os}");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Unable to acquire DepotDownloader: there is no version that supports the operating system {os}, or this program does not support running DepotDownloader on that operating system");
                return;
            }

            string downloadUrl =
                $"https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_{DDVersion}/DepotDownloader-{os}-{arch.ToString().ToLower()}.zip";

            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(downloadUrl);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms);

            ZipFile.ExtractToDirectory(ms, depotDownloaderPath);
        }
    }

    static void AddShortcutToProfile(string profileName)
    {
        var profile = profiles[profileName];

        List<string> data = [
            $"\"{Path.Join("..", "..", "instances", profile.GameVersion, "Risk of Rain 2")}\" {GetProfileLaunchArguments(profileName)}",
            ""
        ];

        // just in case
        Directory.CreateDirectory(Path.Combine(Folder, "profiles", profileName));

        if(os == osWindows)
            File.WriteAllText(Path.Combine(Folder, "profiles", profileName, "launch-game.bat"), string.Join("\r\n", data));

        if(os == osLinux)
            File.WriteAllText(Path.Combine(Folder, "profiles", profileName, "launch-game.sh"), string.Join("\n", data));
    }

    private static void CheckProfilesDirty(bool log = false)
    {
        instances.Clear();
        foreach(var item in Directory.EnumerateDirectories(Path.Combine(Folder, "instances")))
        {
            var path = item.Split(Path.DirectorySeparatorChar)[^1];
            if(RoR2Versions.Versions.Contains(path))
            {
                instances.Add(path);
            }
        }

        if(!profilesDirty) return;

        profiles.Clear();
        foreach(var item in Directory.EnumerateDirectories(Path.Combine(Folder, "profiles")))
        {
            string path = Path.Combine(item, "profile.json");
            if(File.Exists(path))
            {
                try
                {
                    if(log)
                        Console.WriteLine($"Reading profile \"{item}\"...");

                    var profile = JsonSerializer.Deserialize<RoR2Profile>(File.ReadAllText(path), RoR2Profile.SerializerOptions);

                    int plugins = 0;
                    foreach(var fullPath in Directory.EnumerateDirectories(Path.Combine(item, "BepInEx", "plugins")))
                    {
                        if(!Directory.EnumerateFileSystemEntries(fullPath).Any())
                            continue;

                        string manifestPath = Path.Combine(fullPath, "manifest.json");
                        if(File.Exists(manifestPath))
                        {
                            profile.Mods.Add(JsonSerializer.Deserialize<RoR2Plugin>(File.ReadAllText(manifestPath), RoR2Profile.SerializerOptions));
                            plugins++;
                        }

                        if(fullPath.EndsWith("MMHOOK") && File.Exists(Path.Combine(fullPath, "MMHOOK_RoR2.dll")))
                        {
                            plugins++;
                        }
                    }

                    int patchers = 0;
                    foreach(var fullPath in Directory.EnumerateDirectories(Path.Combine(item, "BepInEx", "patchers")))
                    {
                        if(!Directory.EnumerateFileSystemEntries(fullPath).Any())
                            continue;

                        patchers++;
                    }

                    if(log)
                        Console.WriteLine($"Profile has {plugins} plugins and {patchers} patchers");

                    string name = item.Split(Path.DirectorySeparatorChar)[^1];

                    profiles.Add(name, profile);

                    AddShortcutToProfile(name);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Failed to read profile \"{item}\", skipping.\nCaused by: {e}");
                    continue;
                }
            }
        }

        profilesDirty = false;
    }
}
