using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AsasKit.Modules.Identity;

public sealed class DesignTimeIdentityDbFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Read env to pick the right appsettings.{ENV}.json
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Find solution root and point to the STARTUP project (AsasKit.Api)
        var root = FindSolutionRoot();
        var startup = Path.Combine(root, "backend", "AsasKit.Api");

        var cfg = new ConfigurationBuilder()
            .SetBasePath(startup)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = (cfg["Data:Provider"] ?? "sqlserver").ToLowerInvariant();
        var cs = cfg.GetConnectionString("Default") ?? cfg["Data:ConnectionString"];
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                "No connection string. Set ConnectionStrings:Default or Data:ConnectionString.");

        var ob = new DbContextOptionsBuilder<IdentityDbContext>();
        switch (provider)
        {
            case "postgres": ob.UseNpgsql(cs); break;
            case "sqlite": ob.UseSqlite(cs); break;
            default: ob.UseSqlServer(cs); break;
        }

        return new IdentityDbContext(ob.Options);
    }

    private static string FindSolutionRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir is not null && !File.Exists(Path.Combine(dir, "AsasKit.sln")))
            dir = Directory.GetParent(dir)?.FullName
                  ?? throw new DirectoryNotFoundException("AsasKit.sln not found.");
        return dir!;
    }
}
