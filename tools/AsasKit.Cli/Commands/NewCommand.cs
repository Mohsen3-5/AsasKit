using System.CommandLine;
using AsasKit.Cli.Services;

namespace AsasKit.Cli.Commands;

internal static class NewCommand
{
    public static Command Build()
    {
        var cmd = new Command("new", "Clone, brand, configure and initialize a new AsasKit-based app");
        var nameArg = new Argument<string>("name", description: "App/solution name (e.g., AsasApp)");
        var dirOpt  = new Option<string>("--dir", () => ".", "Target directory");

        cmd.AddArgument(nameArg);
        cmd.AddOption(dirOpt);

        cmd.SetHandler(async (string projName, string rootDir) =>
                       {
                           await ScaffoldWorkflow.RunAsync(projName, rootDir);
                       }, nameArg, dirOpt);

        return cmd;
    }
}