using System.CommandLine;
using AsasKit.Cli.Commands;

var root = new RootCommand("AsasKit CLI");
root.AddCommand(NewCommand.Build());

return await root.InvokeAsync(args);