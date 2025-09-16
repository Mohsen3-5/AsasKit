using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asas.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asas.Identity.Application
{
    public sealed class IdentitySynchronizer : IHostedService
    {
        private readonly IServiceProvider _sp;
        public IdentitySynchronizer(IServiceProvider sp) => _sp = sp;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AsasIdentityDbContext>();
            await db.Database.MigrateAsync(ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
