using System.CommandLine;
using AsasKit.Cli.Commands;

namespace AsasKit.Cli
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var root = new RootCommand("AsasKit CLI - The ultimate tool for Asas-based modular apps.");

            root.AddCommand(NewCommand.Build());
            root.AddCommand(TemplateCommand.Build());

            return await root.InvokeAsync(args);
        }
    }
}
