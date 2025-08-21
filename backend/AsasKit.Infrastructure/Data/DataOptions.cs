namespace AsasKit.Infrastructure.Data;

public sealed class DataOptions
{
    public string Provider { get; set; } = "Postgres"; // Postgres | SqlServer | Sqlite
    public string? ConnectionString { get; set; }
}