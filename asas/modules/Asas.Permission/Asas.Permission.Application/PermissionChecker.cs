
using Asas.Permission.Contracts;
using Asas.Permission.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Asas.Identity.Application.Contracts;
namespace Asas.Permission.Application
{
    public sealed class PermissionChecker : IPermissionChecker
    {
        private readonly PermissionDbContext _db;
        private readonly IUserRoleService _userRoles; // from Identity module
        private readonly IDistributedCache _cache;

        public PermissionChecker(PermissionDbContext db, IUserRoleService userRoles, IDistributedCache cache)
        { _db = db; _userRoles = userRoles; _cache = cache; }

        public async Task<bool> IsGrantedAsync(Guid userId, string permission, Guid? tenantId, CancellationToken ct = default)
        {
            var cacheKey = $"perm:{tenantId}:{userId}";
            var map = await _cache.GetAsync(cacheKey, ct) is { } bytes
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(bytes)!
                : null;

            if (map is null)
            {
                // Build effective map
                var roleIds = await _userRoles.GetRoleIdsAsync(userId, tenantId, ct);

                var rolePerms = await _db.RolePermissions
                    .Where(rp => rp.TenantId == tenantId && roleIds.Contains(rp.RoleId))
                    .ToListAsync(ct);

                var overrides = await _db.UserPermissionOverrides
                    .Where(u => u.TenantId == tenantId && u.UserId == userId)
                    .ToListAsync(ct);

                map = rolePerms
                    .GroupBy(x => x.PermissionName)
                    .ToDictionary(g => g.Key, g => g.Any(x => x.IsGranted));

                foreach (var o in overrides)
                    if (o.IsGranted.HasValue) map[o.PermissionName] = o.IsGranted.Value;

                var payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(map);
                await _cache.SetAsync(cacheKey, payload, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                }, ct);
            }

            return map.TryGetValue(permission, out var grant) && grant;
        }
    }
}
