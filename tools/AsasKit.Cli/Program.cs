// tools/AsasKit.Cli/Program.cs
// dotnet add tools/AsasKit.Cli package System.CommandLine --version 2.0.0-beta4.22272.1
// dotnet add tools/AsasKit.Cli package Spectre.Console --version 0.47.0

using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;

var root = new RootCommand("AsasKit CLI");

root.AddCommand(NewCmd());

return await root.InvokeAsync(args);

// =============== Commands ===============

static Command NewCmd()
{
    var cmd = new Command("new", "Create a new AsasKit project via interactive wizard");
    var nameArg = new Argument<string>("name", description: "Folder/solution name");
    var dirOpt = new Option<string>("--dir", () => ".", "Where to create the project folder");

    cmd.AddArgument(nameArg);
    cmd.AddOption(dirOpt);

    cmd.SetHandler(async (string projName, string rootDir) =>
    {
        var projectDir = Path.GetFullPath(Path.Combine(rootDir, projName));
        Directory.CreateDirectory(projectDir);

        AnsiConsole.Write(
            new FigletText("AsasKit").Centered().Color(Color.Gold1));

        AnsiConsole.MarkupLine("[bold]Let's set up your project[/] ✨");

        // 1) Provider selection
        var provider = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a [yellow]database provider[/]:")
                .AddChoices("Sqlite", "SqlServer", "Postgres"));

        // 2) Gather provider-specific settings
        var cfg = new CliConfig
        {
            ApiProject = "backend/AsasKit.Api/AsasKit.Api.csproj",
            MigrationsProject = "backend/AsasKit.Infrastructure/AsasKit.Infrastructure.csproj",
            Provider = provider,
            EfToolsVersion = "9.0.8"
        };

        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                {
                    var dbFile = AnsiConsole.Ask<string>(
                        "SQLite file name (no path) [grey](default: {0}.db)[/]:", $"{projName}.db");

                    cfg.ConnectionString = $"Data Source=./{dbFile}";
                    break;
                }

            case "sqlserver":
                {
                    var mode = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("SQL Server mode:")
                            .AddChoices("LocalDB (Windows dev)", "Docker container", "Custom connection string"));

                    var dbName = AnsiConsole.Ask<string>(
                        "Database name [grey](default: {0})[/]:", ToSafeDbName(projName));

                    if (mode.StartsWith("LocalDB"))
                    {
                        cfg.ConnectionString =
                            $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Encrypt=False";
                    }
                    else if (mode.StartsWith("Docker"))
                    {
                        var hostPort = AnsiConsole.Ask<int>("Host port to map [grey](default: 11433)[/]:", 11433);
                        var saPass = AskPassword("SA password [grey](default: auto strong)[/]:")
                                     ?? GenerateStrongPassword();

                        // Try to run Docker (best-effort)
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
                        var dbName = AnsiConsole.Ask<string>(
                            "Database name [grey](default: {0})[/]:", ToSafeDbName(projName));
                        var user = AnsiConsole.Ask<string>("DB username [grey](default: asaskit)[/]:", "asaskit");
                        var pass = AskPassword("DB password [grey](default: asaskit)[/]:") ?? "asaskit";
                        var port = AnsiConsole.Ask<int>("Host port to map [grey](default: 5432)[/]:", 5432);

                        await TryRunDockerPg(port, user, pass, dbName);

                        cfg.ConnectionString =
                            $"Host=localhost;Port={port};Database={dbName};Username={user};Password={pass}";
                    }
                    else
                    {
                        cfg.ConnectionString = AnsiConsole.Ask<string>("Paste Postgres connection string:");
                    }
                    break;
                }
        }

        // 3) Persist config
        var cfgPath = Path.Combine(projectDir, "asaskit.json");
        await File.WriteAllTextAsync(cfgPath, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));

        // Optional: emit a .env for convenience
        var envPath = Path.Combine(projectDir, ".env");
        var env = $"ASPNETCORE_ENVIRONMENT=Development{Environment.NewLine}" +
                  $"Data__Provider={cfg.Provider}{Environment.NewLine}" +
                  $"Data__ConnectionString={cfg.ConnectionString}{Environment.NewLine}";
        await File.WriteAllTextAsync(envPath, env);

        // 4) (Optional) Copy template skeleton if you ship one
        // var templatePath = Path.Combine(AppContext.BaseDirectory, "template");
        // if (Directory.Exists(templatePath)) CopyDir(templatePath, projectDir);

        // 5) Show summary + next steps
        var masked = MaskConnectionString(cfg.ConnectionString ?? "");
        var table = new Table().BorderColor(Color.Grey)
            .AddColumn("Setting").AddColumn("Value")
            .AddRow("Project", projName)
            .AddRow("Path", projectDir)
            .AddRow("Provider", cfg.Provider ?? "")
            .AddRow("Connection", masked);

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("\n[bold]Next steps[/]:");
        AnsiConsole.MarkupLine($"  1) cd [yellow]{projName}[/]");
        AnsiConsole.MarkupLine($"  2) (optional) load env: [grey]$env:ASPNETCORE_ENVIRONMENT='Development'; $env:Data__Provider='{cfg.Provider}'; $env:Data__ConnectionString='(see .env)'[/]");
        AnsiConsole.MarkupLine($"  3) Run EF: [grey]dotnet tool update --global dotnet-ef --version 9.0.8[/]");
        AnsiConsole.MarkupLine($"     [grey]dotnet ef migrations add Init --project backend/AsasKit.Infrastructure --startup-project backend/AsasKit.Api[/]");
        AnsiConsole.MarkupLine($"     [grey]dotnet ef database update      --project backend/AsasKit.Infrastructure --startup-project backend/AsasKit.Api[/]");
        AnsiConsole.MarkupLine($"  4) Start API: [grey]dotnet run --project backend/AsasKit.Api[/]");
        AnsiConsole.MarkupLine($"  5) Health check: [grey]GET http://localhost:5000/health[/]");

        AnsiConsole.MarkupLine("\n[green]Done![/]");

    }, nameArg, dirOpt);

    return cmd;
}

// =============== Helpers ===============

static string ToSafeDbName(string raw)
{
    // MSSQL/Postgres-safe-ish (letters, digits, underscores)
    var s = Regex.Replace(raw, "[^A-Za-z0-9_]", "_");
    if (char.IsDigit(s.FirstOrDefault())) s = "_" + s;
    return string.IsNullOrWhiteSpace(s) ? "asaskit" : s.ToLowerInvariant();
}

static string? AskPassword(string prompt)
{
    var useCustom = Spectre.Console.AnsiConsole.Confirm("Set a custom password?");
    if (!useCustom) return null;
    return Spectre.Console.AnsiConsole.Prompt(
        new TextPrompt<string>(prompt)
            .PromptStyle("green")
            .Secret());
}

static string GenerateStrongPassword()
{
    // simple strong-ish generator for dev containers
    const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%#";
    var rng = Random.Shared;
    return new string(Enumerable.Range(0, 16).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
}

static async Task TryRunDockerSql(int hostPort, string saPassword)
{
    try
    {
        await Exec("docker", $"rm -f asaskit-sql", ignoreExitCode: true);
        await Exec("docker",
            $"run -e \"ACCEPT_EULA=Y\" -e \"MSSQL_SA_PASSWORD={saPassword}\" -p {hostPort}:1433 --name asaskit-sql -d mcr.microsoft.com/mssql/server:2022-latest",
            ignoreExitCode: false);
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
        await Exec("docker", $"rm -f asaskit-pg", ignoreExitCode: true);
        await Exec("docker",
            $"run -e POSTGRES_USER={user} -e POSTGRES_PASSWORD={pass} -e POSTGRES_DB={db} -p {hostPort}:5432 --name asaskit-pg -d postgres:16",
            ignoreExitCode: false);
        AnsiConsole.MarkupLine($"[green]Postgres container up[/] on localhost:{hostPort}");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]Docker PG not started[/]: {ex.Message}");
    }
}

static async Task<int> Exec(string file, string args, bool ignoreExitCode = false)
{
    var psi = new ProcessStartInfo(file, args)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    using var p = Process.Start(psi)!;
    var stdout = await p.StandardOutput.ReadToEndAsync();
    var stderr = await p.StandardError.ReadToEndAsync();
    p.WaitForExit();
    if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout.TrimEnd());
    if (!string.IsNullOrWhiteSpace(stderr)) Console.Error.WriteLine(stderr.TrimEnd());
    if (p.ExitCode != 0 && !ignoreExitCode) throw new Exception($"{file} {args} exited {p.ExitCode}");
    return p.ExitCode;
}

static string MaskConnectionString(string cs)
{
    if (string.IsNullOrWhiteSpace(cs)) return cs;
    // mask passwords in cs strings (pwd=, password=)
    var masked = Regex.Replace(cs, "(?i)(password\\s*=\\s*)([^;]+)", "$1****");
    masked = Regex.Replace(masked, "(?i)(pwd\\s*=\\s*)([^;]+)", "$1****");
    return masked;
}

// =============== Model ===============

file sealed class CliConfig
{
    public string? ApiProject { get; set; }
    public string? MigrationsProject { get; set; }
    public string? Provider { get; set; }
    public string? ConnectionString { get; set; }
    public string? EfToolsVersion { get; set; }
}
