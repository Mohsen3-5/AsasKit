using AsasKit.Application.Abstractions.Persistence;
using AsasKit.Domain.Tenancy;
using AsasKit.Infrastructure.Data;

namespace AsasKit.Infrastructure.Persistence;

public sealed class MembershipRepository(AppDbContext db) : IMembershipRepository
{
    public async Task AddAsync(Membership membership, CancellationToken ct)
    {
        await db.Memberships.AddAsync(membership, ct);
    }
}