using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AsasKit.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var data = new DataOptions();
        cfg.GetSection("Data").Bind(data);
        var provider = (data.Provider ?? "Postgres").Trim().ToLowerInvariant();

        var cs = string.IsNullOrWhiteSpace(data.ConnectionString)
            ? cfg.GetConnectionString("Default")
            : data.ConnectionString;

        if (provider == "sqlite" && string.IsNullOrWhiteSpace(cs))
            cs = "Data Source=asaskit.db";

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        switch (provider)
        {
            case "postgres": builder.UseNpgsql(cs); break;
            case "sqlserver": builder.UseSqlServer(cs); break;
            case "sqlite": builder.UseSqlite(cs); break;
            default: throw new InvalidOperationException($"Unknown provider {provider}");
        }

        return new AppDbContext(builder.Options, new TenantAccessor(Guid.Empty));
    }
}