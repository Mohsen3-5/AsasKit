using System.Text.Json;
using System.Text.Json.Nodes;
using Spectre.Console;
using AsasKit.Cli.Models;
using AsasKit.Cli.Utils;

namespace AsasKit.Cli.Services;

internal static class ScaffoldWorkflow
{
    private const string EfToolsVersion = "9.0.8";

    public static async Task RunAsync(string projName, string rootDir, string? dbOverride = null, string? csOverride = null)
    {
        var appName = TextUtil.ToPascalCase(projName);
        var targetDir = Path.GetFullPath(Path.Combine(rootDir, appName));

        AnsiConsole.Write(new FigletText("AsasKit").Centered().Color(Color.Gold1));
        AnsiConsole.MarkupLine("[bold]Scaffolding your project[/] 🚀");
        AnsiConsole.MarkupLine($"Creating [yellow]{appName}[/] in [grey]{targetDir}[/] ...");

        // Step 1: Execute dotnet new asas-kit
        await ProcessRunner.Exec("dotnet", $"new asas-kit -n {appName} -o \"{targetDir}\"");

        // Step 2: Database and Docker prompts
        CliConfig cfg;
        if (!string.IsNullOrWhiteSpace(dbOverride))
        {
            cfg = new CliConfig { Provider = dbOverride, ConnectionString = csOverride };
            if (string.IsNullOrWhiteSpace(cfg.ConnectionString) && dbOverride.ToLowerInvariant() == "sqlite")
                cfg.ConnectionString = $"Data Source=./{appName}.db";
        }
        else
        {
            cfg = await PromptForConfigAsync(appName);
        }

        cfg.ApiProject = $"{appName}.Api/{appName}.Api.csproj";
        cfg.MigrationsProject = $"{appName}.Infrastructure/{appName}.Infrastructure.csproj";
        cfg.EfToolsVersion = EfToolsVersion;

        await SaveConfigAsync(targetDir, cfg);
        await WriteEnvAsync(targetDir, cfg);
        await PatchLaunchSettingsAsync(targetDir, appName, cfg);
        await PatchAppSettingsAsync(targetDir, appName, cfg);

        var env = BuildEnv(cfg);
        AnsiConsole.MarkupLine("[grey]Restoring dependencies...[/]");
        await ProcessRunner.Exec("dotnet", "restore", env, targetDir);

        AnsiConsole.MarkupLine("[grey]Ensuring dotnet-ef is ready...[/]");
        await ProcessRunner.Exec("dotnet", $"tool update --global dotnet-ef --version {EfToolsVersion}");

        AnsiConsole.MarkupLine("[grey]Checking migrations...[/]");
        var migList = await ProcessRunner.ExecCapture(
            "dotnet",
            $"ef migrations list --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\" --context AppDbContext",
            env, targetDir, ignoreExitCode: true);

        if (!migList.stdout.Contains("Init", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[grey]Adding initial migration...[/]");
            await ProcessRunner.Exec("dotnet",
                $"ef migrations add Init --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\" --context AppDbContext",
                env, targetDir);
        }

        AnsiConsole.MarkupLine("[grey]Updating database...[/]");
        await ProcessRunner.Exec("dotnet",
            $"ef database update --project \"{cfg.MigrationsProject}\" --startup-project \"{cfg.ApiProject}\" --context AppDbContext",
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
                            .AddChoices("LocalDB (Windows dev)", "Docker container", "Manual configuration", "Paste connection string"));

                    var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", TextUtil.ToSafeDbName(appName));

                    if (mode.StartsWith("LocalDB"))
                    {
                        cfg.ConnectionString = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Encrypt=False";
                    }
                    else if (mode.StartsWith("Docker"))
                    {
                        var hostPort = AnsiConsole.Ask<int>("Host port to map [grey](default: 11433)[/]:", 11433);
                        var saPass = AskPassword("SA password [grey](default: auto strong)[/]:") ?? GenerateStrongPassword();
                        await TryRunDockerSqlAsync(hostPort, saPass);
                        cfg.ConnectionString = $"Server=localhost,{hostPort};Database={dbName};User Id=sa;Password={saPass};Encrypt=True;TrustServerCertificate=True";
                    }
                    else if (mode.StartsWith("Manual"))
                    {
                        var host = AnsiConsole.Ask<string>("Host [grey](default: localhost)[/]:", "localhost");
                        var port = AnsiConsole.Ask<int>("Port [grey](default: 1433)[/]:", 1433);
                        var user = AnsiConsole.Ask<string>("Username [grey](default: sa)[/]:", "sa");
                        var pass = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());
                        cfg.ConnectionString = $"Server={host},{port};Database={dbName};User Id={user};Password={pass};Encrypt=True;TrustServerCertificate=True";
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
                            .AddChoices("Docker container", "Manual configuration", "Paste connection string"));

                    if (mode.StartsWith("Docker"))
                    {
                        var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", TextUtil.ToSafeDbName(appName));
                        var user = AnsiConsole.Ask<string>("DB username [grey](default: asaskit)[/]:", "asaskit");
                        var pass = AskPassword("DB password [grey](default: asaskit)[/]:") ?? "asaskit";
                        var port = AnsiConsole.Ask<int>("Host port to map [grey](default: 5432)[/]:", 5432);
                        await TryRunDockerPgAsync(port, user, pass, dbName);
                        cfg.ConnectionString = $"Host=localhost;Port={port};Database={dbName};Username={user};Password={pass}";
                    }
                    else if (mode.StartsWith("Manual"))
                    {
                        var host = AnsiConsole.Ask<string>("Host [grey](default: localhost)[/]:", "localhost");
                        var port = AnsiConsole.Ask<int>("Port [grey](default: 5432)[/]:", 5432);
                        var dbName = AnsiConsole.Ask<string>("Database name [grey](default: {0})[/]:", TextUtil.ToSafeDbName(appName));
                        var user = AnsiConsole.Ask<string>("Username [grey](default: postgres)[/]:", "postgres");
                        var pass = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());
                        cfg.ConnectionString = $"Host={host};Port={port};Database={dbName};Username={user};Password={pass}";
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
        var apiDir = Path.Combine(dir, $"{appName}.Api");
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

    private static async Task PatchAppSettingsAsync(string dir, string appName, CliConfig cfg)
    {
        var apiDir = Path.Combine(dir, $"{appName}.Api");
        var path = Path.Combine(apiDir, "appsettings.Development.json");
        if (!File.Exists(path)) return;

        var root = JsonNode.Parse(await File.ReadAllTextAsync(path)) as JsonObject;
        if (root == null) return;

        // Patch ConnectionStrings:Default
        var cs = root["ConnectionStrings"] as JsonObject ?? new JsonObject();
        cs["Default"] = cfg.ConnectionString;
        root["ConnectionStrings"] = cs;

        // Patch Data:Provider and Data:ConnectionString
        var data = root["Data"] as JsonObject ?? new JsonObject();
        data["Provider"] = cfg.Provider?.ToLowerInvariant();
        data["ConnectionString"] = cfg.ConnectionString;
        root["Data"] = data;

        await File.WriteAllTextAsync(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    // ---------- EF / Docker ----------
    private static Dictionary<string, string> BuildEnv(CliConfig cfg) => new()
    {
        ["ASPNETCORE_ENVIRONMENT"] = "Development",
        ["Data__Provider"] = cfg.Provider ?? "Sqlite",
        ["Data__ConnectionString"] = cfg.ConnectionString ?? "",
        ["ConnectionStrings__Default"] = cfg.ConnectionString ?? ""
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
        AnsiConsole.MarkupLine($"  [grey]dotnet run --project {appName}.Api/{appName}.Api.csproj[/]");
        AnsiConsole.MarkupLine($"  [grey]GET http://localhost:5000/health[/]");
    }
}
