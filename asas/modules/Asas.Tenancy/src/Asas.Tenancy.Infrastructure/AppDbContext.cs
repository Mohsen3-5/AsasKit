using Microsoft.EntityFrameworkCore;

namespace Asas.Tenancy.Infrastructure; 

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Pull in any IEntityTypeConfiguration<T> you define in this assembly
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

