using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asas.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Asas.Tenancy.Infrastructure.EF;
public interface ITenantStore
{
    Task<TenantDto?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenantDto?> FindByHostAsync(string host, CancellationToken ct = default);
}

public sealed class EfTenantStore : ITenantStore
{
    private readonly TenancyDbContext _db;
    public EfTenantStore(TenancyDbContext db) => _db = db;

    public async Task<TenantDto?> FindByHostAsync(string host, CancellationToken ct = default)
    => await _db.Set<Tenant>()
            .AsNoTracking()
            .Where(t => t.Host == host && t.IsActive)
            .Select(t => new TenantDto(t.Identifier, t.Name))
            .FirstOrDefaultAsync(ct);
}

    public async Task<TenantDto?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<Tenant>()
            .AsNoTracking()
            .Where(t => t.Id == id && t.IsActive)
            .Select(t => new TenantDto(t.Identifier, t.Name))
            .FirstOrDefaultAsync(ct);
}

public sealed record TenantDto(string Id, string Name);
