using AsasKit.Application.Abstractions.Persistence;
using AsasKit.Domain.Tenancy;
using AsasKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AsasKit.Infrastructure.Persistence;

public sealed class TenantRepository(AppDbContext db) : ITenantRepository
{
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.Tenants.AnyAsync(t => t.Slug == slug, ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct)
    {
        await db.Tenants.AddAsync(tenant, ct);
    }
}