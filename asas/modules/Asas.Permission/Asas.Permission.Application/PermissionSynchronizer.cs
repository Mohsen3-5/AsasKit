using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asas.Permission.Contracts;
using Asas.Permission.Domain.Entity;
using Asas.Permission.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asas.Permission.Application
{
    public sealed class PermissionSynchronizer : IHostedService
    {
        private readonly IServiceProvider _sp;
        public PermissionSynchronizer(IServiceProvider sp) => _sp = sp;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
            await db.Database.MigrateAsync(ct); 

            var defs = await scope.ServiceProvider.GetRequiredService<IPermissionDefinitionStore>().GetAllAsync(ct);
     

            var existing = await db.AsasPermission.ToDictionaryAsync(x => new { x.TenantId, x.Name }, ct);
            foreach (var def in defs)
            {
                var key = new { def.TenantId, def.Name };
                if (!existing.ContainsKey(key))
                    db.AsasPermission.Add(new AsasPermission { Name = def.Name, DisplayName = def.DisplayName, Description = def.Description, Group = def.Group, TenantId = def.TenantId });
            }
            await db.SaveChangesAsync(ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
