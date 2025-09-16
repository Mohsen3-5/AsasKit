
using Asas.Tenancy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asas.Tenancy.Application
{
    public sealed class TenancySynchronizer : IHostedService
    {
        private readonly IServiceProvider _sp;
        public TenancySynchronizer(IServiceProvider sp) => _sp = sp;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TenancyDbContext>();
            await db.Database.MigrateAsync(ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
