using System.Text.Json;
using System.Text.Json.Nodes;
using Spectre.Console;
using AsasKit.Cli.Models;
using AsasKit.Cli.Utils;

namespace AsasKit.Cli.Services;

internal static class ScaffoldWorkflow
{
    private const string RepoUrl         = "https://github.com/Mohsen3-5/AsasKit.git";
    private const string EfToolsVersion  = "9.0.8";

    public static async Task RunAsync(string projName, string rootDir)
    {
        var appName  = TextUtil.ToPascalCase(projName);
        var targetDir= Path.GetFullPath(Path.Combine(rootDir, appName));
        Directory.CreateDirectory(targetDir);

        AnsiConsole.Write(new FigletText("AsasKit").Centered().Color(Color.Gold1));
        AnsiConsole.MarkupLine("[bold]Scaffolding your project[/] 🚀");
        AnsiConsole.MarkupLine($"Cloning into [yellow]{targetDir}[/] ...");

        await ProcessRunner.Exec("git", $"clone --depth 1 {RepoUrl} \"{targetDir}\"");
        TryDelete(Path.Combine(targetDir, ".git")); // de-template

        await ApplyBrandingAsync(targetDir, appName);

        var cfg = await PromptForConfigAsync(appName);
        cfg.ApiProject        = $"backend/{appName}.Api/{appName}.Api.csproj";
        cfg.MigrationsProject = $"backend/{appName}.Infrastructure/{appName}.Infrastructure.csproj";
        cfg.EfToolsVersion    = EfToolsVersion;

        await SaveConfigAsync(targetDir, cfg);
        await WriteEnvAsync(targetDir, cfg);
        await PatchLaunchSettingsAsync(targetDir, appName, cfg);

        var env = BuildEnv(cfg);
        await ProcessRunner.Exec("dotnet", "restore", env, targetDir);
        await ProcessRunner.Exec("dotnet", $"tool update --global dotnet-ef --version {EfToolsVersion}");

        var migList = await ProcessRunner.ExecCapture(
            "dotnet",
            $"ef migrations list --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\"",
            env, targetDir, ignoreExitCode: true);

        if (!migList.stdout.Contains("Init", StringComparison.OrdinalIgnoreCase))
        {
            await ProcessRunner.Exec("dotnet",
                $"ef migrations add Init --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\"",
                env, targetDir);
        }

        await ProcessRunner.Exec("dotnet",
            $"ef database update --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\"",
            env, targetDir);

        PrintSummary(appName, cfg, targetDir);
    }

    // ---------- prompts ----------
    private static async Task<CliConfig> PromptForConfigAsync(string appName)
    {
        var provider = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a [yellow]database provider[/]:")
                .AddChoices("Sqlite", "SqlServer", "Postgres"));

        var cfg = new CliConfig { Provider = provider };

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

                var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", TextUtil.ToSafeDbName(appName));

                if (mode.StartsWith("LocalDB"))
                {
                    cfg.ConnectionString =
                        $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Encrypt=False";
                }
                else if (mode.StartsWith("Docker"))
                {
                    var hostPort = AnsiConsole.Ask<int>("Host port to map [grey](default: 11433)[/]:", 11433);
                    var saPass   = AskPassword("SA password [grey](default: auto strong)[/]:") ?? GenerateStrongPassword();
                    await TryRunDockerSqlAsync(hostPort, saPass);
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
                    var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", TextUtil.ToSafeDbName(appName));
                    var user   = AnsiConsole.Ask<string>("DB username [grey](default: asaskit)[/]:", "asaskit");
                    var pass   = AskPassword("DB password [grey](default: asaskit)[/]:") ?? "asaskit";
                    var port   = AnsiConsole.Ask<int>("Host port to map [grey](default: 5432)[/]:", 5432);
                    await TryRunDockerPgAsync(port, user, pass, dbName);
                    cfg.ConnectionString = $"Host=localhost;Port={port};Database={dbName};Username={user};Password={pass}";
                }
                else
                {
                    cfg.ConnectionString = AnsiConsole.Ask<string>("Paste Postgres connection string:");
                }
                break;
            }
        }

        return await Task.FromResult(cfg);
    }

    // ---------- branding ----------
    private static async Task ApplyBrandingAsync(string root, string app)
    {
        var toolsDir = Path.Combine(root, "tools");
        if (Directory.Exists(toolsDir)) Directory.Delete(toolsDir, true);

        var backend = Path.Combine(root, "backend");
        var modules = new[] { "Api", "Application", "Domain", "Infrastructure", "Shared" };

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

        var patterns = new[] { "*.cs", "*.csproj", "*.sln", "*.json" };
        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
            {
                var txt = await File.ReadAllTextAsync(file);
                txt = txt.Replace("namespace AsasKit", $"namespace {app}");
                txt = txt.Replace("using AsasKit.", $"using {app}.");
                txt = txt.Replace("AsasKit.", $"{app}.");
                txt = System.Text.RegularExpressions.Regex.Replace(txt, @"\bAsasKit\b", app);
                await File.WriteAllTextAsync(file, txt);
            }
        }

        var oldSln = Path.Combine(root, "AsasKit.sln");
        if (File.Exists(oldSln)) File.Delete(oldSln);

        await ProcessRunner.Exec("dotnet", $"new sln -n {app}", cwd: root);
        var csprojs = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojs)
            await ProcessRunner.Exec("dotnet", $"sln {app}.sln add \"{csproj}\"", cwd: root);
    }

    // ---------- file ops ----------
    private static async Task SaveConfigAsync(string dir, CliConfig cfg)
    {
        var path = Path.Combine(dir, "asaskit.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task WriteEnvAsync(string dir, CliConfig cfg)
    {
        var envPath = Path.Combine(dir, ".env");
        var env = $"ASPNETCORE_ENVIRONMENT=Development{Environment.NewLine}" +
                  $"Data__Provider={cfg.Provider}{Environment.NewLine}" +
                  $"Data__ConnectionString={cfg.ConnectionString}{Environment.NewLine}" +
                  $"ConnectionStrings__Default={cfg.ConnectionString}{Environment.NewLine}";
        await File.WriteAllTextAsync(envPath, env);
    }

    private static async Task PatchLaunchSettingsAsync(string dir, string appName, CliConfig cfg)
    {
        var apiDir = Path.Combine(dir, "backend", $"{appName}.Api");
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

        prof["commandName"]    = "Project";
        prof["applicationUrl"] = "http://localhost:5000";
        prof["launchBrowser"]  = true;

        var env = prof["environmentVariables"] as JsonObject ?? new JsonObject();
        env["ASPNETCORE_ENVIRONMENT"]   = "Development";
        env["Data__Provider"]           = cfg.Provider;
        env["Data__ConnectionString"]   = cfg.ConnectionString;
        env["ConnectionStrings__Default"]= cfg.ConnectionString;
        prof["environmentVariables"] = env;

        await File.WriteAllTextAsync(lsPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    // ---------- EF / Docker ----------
    private static Dictionary<string,string> BuildEnv(CliConfig cfg) => new()
    {
        ["ASPNETCORE_ENVIRONMENT"]    = "Development",
        ["Data__Provider"]            = cfg.Provider ?? "Sqlite",
        ["Data__ConnectionString"]    = cfg.ConnectionString ?? "",
        ["ConnectionStrings__Default"]= cfg.ConnectionString ?? ""
    };

    private static async Task TryRunDockerSqlAsync(int hostPort, string saPassword)
    {
        try
        {
            await ProcessRunner.Exec("docker", "rm -f asaskit-sql", ignoreExitCode: true);
            await ProcessRunner.Exec("docker",
                $"run -e \"ACCEPT_EULA=Y\" -e \"MSSQL_SA_PASSWORD={saPassword}\" -p {hostPort}:1433 --name asaskit-sql -d mcr.microsoft.com/mssql/server:2022-latest");
            AnsiConsole.MarkupLine($"[green]SQL Server container up[/] on localhost:{hostPort}");
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[yellow]Docker SQL not started[/]: {ex.Message}"); }
    }

    private static async Task TryRunDockerPgAsync(int hostPort, string user, string pass, string db)
    {
        try
        {
            await ProcessRunner.Exec("docker", "rm -f asaskit-pg", ignoreExitCode: true);
            await ProcessRunner.Exec("docker",
                $"run -e POSTGRES_USER={user} -e POSTGRES_PASSWORD={pass} -e POSTGRES_DB={db} -p {hostPort}:5432 --name asaskit-pg -d postgres:16");
            AnsiConsole.MarkupLine($"[green]Postgres container up[/] on localhost:{hostPort}");
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[yellow]Docker PG not started[/]: {ex.Message}"); }
    }

    private static string? AskPassword(string prompt)
    {
        var useCustom = AnsiConsole.Confirm("Set a custom password?");
        if (!useCustom) return null;
        return AnsiConsole.Prompt(new TextPrompt<string>(prompt).PromptStyle("green").Secret());
    }

    private static string GenerateStrongPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%#";
        var rng = Random.Shared;
        return new string(Enumerable.Range(0, 16).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }

    // ---------- misc ----------
    private static void TryDelete(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, true); }
        catch { /* ignore */ }
    }

    private static void PrintSummary(string appName, CliConfig cfg, string targetDir)
    {
        var masked = TextUtil.MaskConnectionString(cfg.ConnectionString ?? "");
        var table = new Spectre.Console.Table().BorderColor(Color.Grey)
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
    }
}
