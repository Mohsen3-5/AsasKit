using System.CommandLine;
using AsasKit.Cli.Utils;

namespace AsasKit.Cli.Commands;

internal static class TemplateCommand
{
    public static Command Build()
    {
        var cmd = new Command("template", "Manage AsasKit templates (install, update, list)");

        var install = new Command("install", "Install or update the AsasKit template from local or NuGet")
        {
            new Argument<string>("path", () => ".", "Path to the template folder or NuGet package name")
        };
        install.SetHandler(async (string path) =>
        {
            Console.WriteLine($"Installing template from {path}...");
            await ProcessRunner.Exec("dotnet", $"new install \"{path}\" --force");
        }, install.Arguments[0] as Argument<string>);

        var uninstall = new Command("uninstall", "Uninstall the AsasKit template");
        uninstall.SetHandler(async () =>
        {
            Console.WriteLine("Uninstalling AsasKit template...");
            await ProcessRunner.Exec("dotnet", "new uninstall AsasKit.Starter");
        });

        var list = new Command("list", "List installed templates");
        list.SetHandler(async () =>
        {
            await ProcessRunner.Exec("dotnet", "new list");
        });

        cmd.AddCommand(install);
        cmd.AddCommand(uninstall);
        cmd.AddCommand(list);

        return cmd;
    }
}
