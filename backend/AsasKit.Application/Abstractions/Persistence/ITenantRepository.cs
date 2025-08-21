using AsasKit.Domain.Tenancy;

namespace AsasKit.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);
    Task AddAsync(Tenant tenant, CancellationToken ct);
}