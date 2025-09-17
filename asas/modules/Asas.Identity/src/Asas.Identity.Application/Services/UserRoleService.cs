using Asas.Identity.Application.Contracts;
using Asas.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Asas.Identity.Application.Services;
public sealed class UserRoleService : IUserRoleService
{
    private readonly AsasIdentityDbContext _db;

    public UserRoleService(AsasIdentityDbContext db) => _db = db;

    public async Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, int? tenantId = null, CancellationToken ct = default)
    {
        // Default ASP.NET Core Identity join table (no TenantId column by default).
        // If you later add TenantId to user-role membership, add "&& ur.TenantId == tenantId" here.
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);
    }
}