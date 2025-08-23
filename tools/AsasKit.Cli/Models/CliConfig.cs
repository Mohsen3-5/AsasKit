namespace AsasKit.Cli.Models;

internal sealed class CliConfig
{
    public string? ApiProject { get; set; }
    public string? MigrationsProject { get; set; }
    public string? Provider { get; set; }
    public string? ConnectionString { get; set; }
    public string  EfToolsVersion { get; set; } = "9.0.8";
}