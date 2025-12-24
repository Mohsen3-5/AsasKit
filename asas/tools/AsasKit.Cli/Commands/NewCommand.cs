using System.CommandLine;
using AsasKit.Cli.Services;

namespace AsasKit.Cli.Commands;

internal static class NewCommand
{
    public static Command Build()
    {
        var cmd = new Command("new", "Clone, brand, configure and initialize a new AsasKit-based app");
        var nameArg = new Argument<string>("name", description: "App/solution name (e.g., AsasApp)");
        var dirOpt = new Option<string>("--dir", () => ".", "Target directory");
        var dbOpt = new Option<string>("--db", "Database provider (Sqlite, SqlServer, Postgres)");
        var csOpt = new Option<string>("--cs", "Custom connection string");

        cmd.AddArgument(nameArg);
        cmd.AddOption(dirOpt);
        cmd.AddOption(dbOpt);
        cmd.AddOption(csOpt);

        cmd.SetHandler(async (string projName, string rootDir, string? db, string? cs) =>
        {
            await ScaffoldWorkflow.RunAsync(projName, rootDir, db, cs);
        }, nameArg, dirOpt, dbOpt, csOpt);

        return cmd;
    }
}