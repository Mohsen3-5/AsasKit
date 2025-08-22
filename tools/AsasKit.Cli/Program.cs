// tools/AsasKit.Cli/Program.cs
// dotnet add tools/AsasKit.Cli package System.CommandLine --version 2.0.0-beta4.22272.1
// dotnet add tools/AsasKit.Cli package Spectre.Console   --version 0.47.0

using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Spectre.Console;
using System.Linq;

const string REPO_URL = "https://github.com/Mohsen3-5/AsasKit.git";
const string EF_TOOLS_VERSION = "9.0.8";

var root = new RootCommand("AsasKit CLI");
root.AddCommand(NewCmd());
return await root.InvokeAsync(args);

// ================= Commands =================

static Command NewCmd()
{
    var cmd = new Command("new", "Clone, brand, configure and initialize a new AsasKit-based app");
    var nameArg = new Argument<string>("name", description: "App/solution name (e.g., AsasApp)");
    var dirOpt = new Option<string>("--dir", () => ".", "Target directory");

    cmd.AddArgument(nameArg);
    cmd.AddOption(dirOpt);

    cmd.SetHandler(async (string projName, string rootDir) =>
    {
        var appName = ToPascalCase(projName);
        var targetDir = Path.GetFullPath(Path.Combine(rootDir, appName));
        Directory.CreateDirectory(targetDir);

        AnsiConsole.Write(new FigletText("AsasKit").Centered().Color(Color.Gold1));
        AnsiConsole.MarkupLine("[bold]Scaffolding your project[/] 🚀");
        AnsiConsole.MarkupLine($"Cloning into [yellow]{targetDir}[/] ...");

        // 1) Clone template repo
        await Exec("git", $"clone --depth 1 {REPO_URL} \"{targetDir}\"");
        TryDelete(Path.Combine(targetDir, ".git")); // de-template

        // 2) Brand: rename AsasKit.* → {appName}.*
        await ApplyBranding(targetDir, appName);

        // 3) Choose provider & construct connection string (and optional Docker DB)
        var provider = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a [yellow]database provider[/]:")
                .AddChoices("Sqlite", "SqlServer", "Postgres"));

        var cfg = new CliConfig
        {
            ApiProject = $"backend/{appName}.Api/{appName}.Api.csproj",
            MigrationsProject = $"backend/{appName}.Infrastructure/{appName}.Infrastructure.csproj",
            Provider = provider,
            EfToolsVersion = EF_TOOLS_VERSION
        };

        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                {
                    var dbFile = AnsiConsole.Ask<string>("SQLite file name [grey](default: {0}.db)[/]:", $"{appName}.db");
                    cfg.ConnectionString = $"Data Source=./{dbFile}";
                    break;
                }

            case "sqlserver":
                {
                    var mode = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("SQL Server mode:")
                            .AddChoices("LocalDB (Windows dev)", "Docker container", "Custom connection string"));

                    var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", ToSafeDbName(appName));

                    if (mode.StartsWith("LocalDB"))
                    {
                        cfg.ConnectionString =
                            $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Encrypt=False";
                    }
                    else if (mode.StartsWith("Docker"))
                    {
                        var hostPort = AnsiConsole.Ask<int>("Host port to map [grey](default: 11433)[/]:", 11433);
                        var saPass = AskPassword("SA password [grey](default: auto strong)[/]:") ?? GenerateStrongPassword();
                        await TryRunDockerSql(hostPort, saPass);
                        cfg.ConnectionString =
                            $"Server=localhost,{hostPort};Database={dbName};User Id=sa;Password={saPass};Encrypt=True;TrustServerCertificate=True";
                    }
                    else
                    {
                        cfg.ConnectionString = AnsiConsole.Ask<string>("Paste SQL Server connection string:");
                    }
                    break;
                }

            case "postgres":
                {
                    var mode = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Postgres mode:")
                            .AddChoices("Docker container", "Custom connection string"));

                    if (mode.StartsWith("Docker"))
                    {
                        var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", ToSafeDbName(appName));
                        var user = AnsiConsole.Ask<string>("DB username [grey](default: asaskit)[/]:", "asaskit");
                        var pass = AskPassword("DB password [grey](default: asaskit)[/]:") ?? "asaskit";
                        var port = AnsiConsole.Ask<int>("Host port to map [grey](default: 5432)[/]:", 5432);
                        await TryRunDockerPg(port, user, pass, dbName);
                        cfg.ConnectionString = $"Host=localhost;Port={port};Database={dbName};Username={user};Password={pass}";
                    }
                    else
                    {
                        cfg.ConnectionString = AnsiConsole.Ask<string>("Paste Postgres connection string:");
                    }
                    break;
                }
        }

        // 4) Persist config
        await File.WriteAllTextAsync(Path.Combine(targetDir, "asaskit.json"),
            JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));

        // 5) Write .env + patch launchSettings.json
        await WriteEnv(targetDir, cfg);
        await PatchLaunchSettings(targetDir, appName, cfg);

        // 6) Restore + align EF tools + create/apply migrations
        var env = EnvFor(cfg);
        await Exec("dotnet", "restore", env, targetDir);
        await Exec("dotnet", $"tool update --global dotnet-ef --version {EF_TOOLS_VERSION}");

        var migList = await ExecCapture("dotnet",
            $"ef migrations list --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\"",
            env, targetDir, ignoreExitCode: true);

        if (!migList.stdout.Contains("Init", StringComparison.OrdinalIgnoreCase))
        {
            await Exec("dotnet",
                $"ef migrations add Init --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\"",
                env, targetDir);
        }

        await Exec("dotnet",
            $"ef database update --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\"",
            env, targetDir);

        // 7) Summary
        var masked = MaskConnectionString(cfg.ConnectionString ?? "");
        var table = new Table().BorderColor(Color.Grey)
            .AddColumn("Setting").AddColumn("Value")
            .AddRow("Solution", $"{appName}.sln")
            .AddRow("Provider", cfg.Provider ?? "")
            .AddRow("Connection", masked)
            .AddRow("Path", targetDir);
        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("\n[green]Ready.[/] Next:");
        AnsiConsole.MarkupLine($"  [grey]cd \"{targetDir}\"[/]");
        AnsiConsole.MarkupLine($"  [grey]dotnet run --project backend/{appName}.Api/{appName}.Api.csproj[/]");
        AnsiConsole.MarkupLine($"  [grey]GET http://localhost:5000/health[/]");

    }, nameArg, dirOpt);

    return cmd;
}

// ================= Branding =================

static async Task ApplyBranding(string root, string app)
{
    // remove template tools for consumer repo
    var toolsDir = Path.Combine(root, "tools");
    if (Directory.Exists(toolsDir)) Directory.Delete(toolsDir, true);

    var backend = Path.Combine(root, "backend");
    var modules = new[] { "Api", "Application", "Domain", "Infrastructure", "Shared" };

    // 1) rename folders + csproj names
    foreach (var m in modules)
    {
        var oldDir = Path.Combine(backend, $"AsasKit.{m}");
        var newDir = Path.Combine(backend, $"{app}.{m}");
        if (Directory.Exists(oldDir))
        {
            Directory.Move(oldDir, newDir);

            var oldProj = Path.Combine(newDir, $"AsasKit.{m}.csproj");
            var newProj = Path.Combine(newDir, $"{app}.{m}.csproj");
            if (File.Exists(oldProj)) File.Move(oldProj, newProj);
        }
    }

    // 2) text replacement across code/refs/solution
    var patterns = new[] { "*.cs", "*.csproj", "*.sln", "*.json" };
    foreach (var pattern in patterns)
    {
        foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
        {
            var txt = await File.ReadAllTextAsync(file);

            // namespaces & usings first
            txt = txt.Replace("namespace AsasKit", $"namespace {app}");
            txt = txt.Replace("using AsasKit.", $"using {app}.");

            // then general identifiers/paths
            txt = txt.Replace("AsasKit.", $"{app}.");
            txt = Regex.Replace(txt, @"\bAsasKit\b", app);

            await File.WriteAllTextAsync(file, txt);
        }
    }

    // 3) rebuild solution
    var oldSln = Path.Combine(root, "AsasKit.sln");
    if (File.Exists(oldSln)) File.Delete(oldSln);

    await Exec("dotnet", $"new sln -n {app}", cwd: root);

    var csprojs = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories);
    foreach (var csproj in csprojs)
        await Exec("dotnet", $"sln {app}.sln add \"{csproj}\"", cwd: root);
}

// ================= Helpers =================

static async Task WriteEnv(string projectDir, CliConfig cfg)
{
    var envPath = Path.Combine(projectDir, ".env");
    var env = $"ASPNETCORE_ENVIRONMENT=Development{Environment.NewLine}" +
              $"Data__Provider={cfg.Provider}{Environment.NewLine}" +
              $"Data__ConnectionString={cfg.ConnectionString}{Environment.NewLine}" +
              $"ConnectionStrings__Default={cfg.ConnectionString}{Environment.NewLine}";
    await File.WriteAllTextAsync(envPath, env);
}

static async Task PatchLaunchSettings(string projectDir, string appName, CliConfig cfg)
{
    var apiDir = Path.Combine(projectDir, "backend", $"{appName}.Api");
    var lsPath = Path.Combine(apiDir, "Properties", "launchSettings.json");
    Directory.CreateDirectory(Path.GetDirectoryName(lsPath)!);

    JsonNode root;
    if (File.Exists(lsPath))
        root = JsonNode.Parse(await File.ReadAllTextAsync(lsPath)) ?? new JsonObject();
    else
        root = new JsonObject();

    var profiles = root["profiles"] as JsonObject ?? new JsonObject();
    root["profiles"] = profiles;

    var profileName = $"{appName}.Api (AsasCtl)";
    var prof = profiles[profileName] as JsonObject ?? new JsonObject();
    profiles[profileName] = prof;

    prof["commandName"] = "Project";
    prof["applicationUrl"] = "http://localhost:5000";
    prof["launchBrowser"] = true;

    var env = prof["environmentVariables"] as JsonObject ?? new JsonObject();
    env["ASPNETCORE_ENVIRONMENT"] = "Development";
    env["Data__Provider"] = cfg.Provider;
    env["Data__ConnectionString"] = cfg.ConnectionString;
    env["ConnectionStrings__Default"] = cfg.ConnectionString;
    prof["environmentVariables"] = env;

    await File.WriteAllTextAsync(lsPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
}

static Dictionary<string, string> EnvFor(CliConfig cfg) => new()
{
    ["ASPNETCORE_ENVIRONMENT"] = "Development",
    ["Data__Provider"] = cfg.Provider ?? "Sqlite",
    ["Data__ConnectionString"] = cfg.ConnectionString ?? "",
    ["ConnectionStrings__Default"] = cfg.ConnectionString ?? ""
};

static string ToPascalCase(string raw)
{
    var parts = Regex.Split(raw, @"[^A-Za-z0-9]+").Where(s => s.Length > 0);
    var pascal = string.Concat(parts.Select(s => char.ToUpper(s[0]) + s.Substring(1)));
    if (string.IsNullOrEmpty(pascal)) pascal = "App";
    if (char.IsDigit(pascal[0])) pascal = "_" + pascal;
    return pascal;
}

static string ToSafeDbName(string raw)
{
    var s = Regex.Replace(raw, "[^A-Za-z0-9_]", "_");
    if (s.Length == 0) s = "asaskit";
    if (char.IsDigit(s[0])) s = "_" + s;
    return s.ToLowerInvariant();
}

static string? AskPassword(string prompt)
{
    var useCustom = AnsiConsole.Confirm("Set a custom password?");
    if (!useCustom) return null;
    return AnsiConsole.Prompt(new TextPrompt<string>(prompt).PromptStyle("green").Secret());
}

static string GenerateStrongPassword()
{
    const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%#";
    var rng = Random.Shared;
    return new string(Enumerable.Range(0, 16).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
}

static async Task TryRunDockerSql(int hostPort, string saPassword)
{
    try
    {
        await Exec("docker", "rm -f asaskit-sql", null, null, true);
        await Exec("docker",
            $"run -e \"ACCEPT_EULA=Y\" -e \"MSSQL_SA_PASSWORD={saPassword}\" -p {hostPort}:1433 --name asaskit-sql -d mcr.microsoft.com/mssql/server:2022-latest");
        AnsiConsole.MarkupLine($"[green]SQL Server container up[/] on localhost:{hostPort}");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]Docker SQL not started[/]: {ex.Message}");
    }
}

static async Task TryRunDockerPg(int hostPort, string user, string pass, string db)
{
    try
    {
        await Exec("docker", "rm -f asaskit-pg", null, null, true);
        await Exec("docker",
            $"run -e POSTGRES_USER={user} -e POSTGRES_PASSWORD={pass} -e POSTGRES_DB={db} -p {hostPort}:5432 --name asaskit-pg -d postgres:16");
        AnsiConsole.MarkupLine($"[green]Postgres container up[/] on localhost:{hostPort}");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]Docker PG not started[/]: {ex.Message}");
    }
}

static async Task Exec(string file, string args, Dictionary<string, string>? env = null, string? cwd = null, bool ignoreExitCode = false)
{
    var psi = new ProcessStartInfo(file, args)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = cwd ?? Environment.CurrentDirectory
    };
    if (env != null) foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;

    using var p = Process.Start(psi)!;
    var stdout = await p.StandardOutput.ReadToEndAsync();
    var stderr = await p.StandardError.ReadToEndAsync();
    p.WaitForExit();

    if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout.TrimEnd());
    if (!string.IsNullOrWhiteSpace(stderr)) Console.Error.WriteLine(stderr.TrimEnd());
    if (p.ExitCode != 0 && !ignoreExitCode) throw new Exception($"{file} {args} exited {p.ExitCode}");
}

static async Task<(string stdout, string stderr, int code)> ExecCapture(string file, string args, Dictionary<string, string>? env = null, string? cwd = null, bool ignoreExitCode = false)
{
    var psi = new ProcessStartInfo(file, args)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        WorkingDirectory = cwd ?? Environment.CurrentDirectory
    };
    if (env != null) foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;

    using var p = Process.Start(psi)!;
    var stdout = await p.StandardOutput.ReadToEndAsync();
    var stderr = await p.StandardError.ReadToEndAsync();
    p.WaitForExit();

    if (p.ExitCode != 0 && !ignoreExitCode) Console.Error.WriteLine(stderr.TrimEnd());
    return (stdout, stderr, p.ExitCode);
}

static void TryDelete(string path)
{
    try { if (Directory.Exists(path)) Directory.Delete(path, true); }
    catch { /* fine */ }
}

static string MaskConnectionString(string cs)
{
    if (string.IsNullOrWhiteSpace(cs)) return cs;
    var masked = Regex.Replace(cs, "(?i)(password\\s*=\\s*)([^;]+)", "$1****");
    masked = Regex.Replace(masked, "(?i)(pwd\\s*=\\s*)([^;]+)", "$1****");
    return masked;
}

// ================= Model =================

file sealed class CliConfig
{
    public string? ApiProject { get; set; }
    public string? MigrationsProject { get; set; }
    public string? Provider { get; set; }
    public string? ConnectionString { get; set; }
    public string? EfToolsVersion { get; set; }
}
