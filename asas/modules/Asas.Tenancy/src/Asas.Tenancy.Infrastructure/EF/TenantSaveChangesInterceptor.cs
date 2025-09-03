using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Asas.Tenancy.Infrastructure.Runtime;

namespace Asas.Tenancy.Infrastructure.Ef;

public sealed class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly TenancyModelOptions _opt;
    public TenantSaveChangesInterceptor(IOptions<TenancyModelOptions> opt) => _opt = opt.Value;

    public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> result)
    { Stamp(e.Context); return base.SavingChanges(e, result); }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    { Stamp(e.Context); return base.SavingChangesAsync(e, result, ct); }

    private void Stamp(DbContext? ctx)
    {
        if (ctx is null) return;
        var tid = TenancyAmbient.CurrentTenantId;
        if (string.IsNullOrWhiteSpace(tid)) return;

        var propName = _opt.TenantIdPropertyName;
        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            var metaProp = entry.Metadata.FindProperty(propName);
            if (metaProp is null) continue; // entity not scoped

            var p = entry.Property(propName);
            if (entry.State == EntityState.Added)
                p.CurrentValue = tid;
            else if (entry.State is EntityState.Modified)
                p.IsModified = false; // prevent TenantId tampering
        }
    }
}
