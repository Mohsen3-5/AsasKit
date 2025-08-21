using AsasKit.Domain.Tenancy;

namespace AsasKit.Application.Abstractions.Persistence;

public interface IMembershipRepository
{
    Task AddAsync(Membership membership, CancellationToken ct);
}