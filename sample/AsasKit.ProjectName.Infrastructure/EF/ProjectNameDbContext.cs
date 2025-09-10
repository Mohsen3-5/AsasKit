using Microsoft.EntityFrameworkCore;

namespace AsasKit.ProjectName.Infrastructure.EF
{
    public class ProjectNameDbContext : DbContext
    {
        public ProjectNameDbContext(DbContextOptions<ProjectNameDbContext> options)
            : base(options)
        {
        }

        // Add DbSet<TEntity> properties here, e.g.:
        // public DbSet<MyEntity> MyEntities { get; set; }
    }
}
