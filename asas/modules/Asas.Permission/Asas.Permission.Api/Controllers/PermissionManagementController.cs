using Asas.Permission.Application;
using Asas.Permission.Contracts;
using Asas.Permission.Domain.Entity;
using Asas.Permission.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asas.Permission.Api.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class PermissionManagementController : ControllerBase
    {
        private readonly PermissionDbContext _db;
        private readonly IPermissionChecker _permissionChecker;

        public PermissionManagementController(
            PermissionDbContext db,
            IPermissionChecker permissionChecker)
        {
            _db = db;
            _permissionChecker = permissionChecker;
        }

        #region Permission Definitions

        /// <summary>
        /// Get all available permissions in the system
        /// </summary>
        [HttpGet]
        [RequiresPermission("Permissions.View")]
        public async Task<IActionResult> GetAllPermissions([FromQuery] int? tenantId)
        {
            var permissions = await _db.AsasPermission
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Group)
                .ThenBy(p => p.DisplayName)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.DisplayName,
                    p.Description,
                    p.Group,
                    p.IsEnabled
                })
                .ToListAsync();

            return Ok(permissions);
        }

        /// <summary>
        /// Get permissions grouped by category
        /// </summary>
        [HttpGet("grouped")]
        [RequiresPermission("Permissions.View")]
        public async Task<IActionResult> GetPermissionsGrouped([FromQuery] int? tenantId)
        {
            var permissions = await _db.AsasPermission
                .Where(p => p.TenantId == tenantId && p.IsEnabled)
                .OrderBy(p => p.Group)
                .ThenBy(p => p.DisplayName)
                .ToListAsync();

            var grouped = permissions
                .GroupBy(p => p.Group)
                .Select(g => new
                {
                    Group = g.Key,
                    Permissions = g.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.DisplayName,
                        p.Description
                    }).ToList()
                })
                .ToList();

            return Ok(grouped);
        }

        /// <summary>
        /// Get a specific permission by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [RequiresPermission("Permissions.View")]
        public async Task<IActionResult> GetPermissionByName(string name, [FromQuery] int? tenantId)
        {
            var permission = await _db.AsasPermission
                .Where(p => p.Name == name && p.TenantId == tenantId)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.DisplayName,
                    p.Description,
                    p.Group,
                    p.IsEnabled
                })
                .FirstOrDefaultAsync();

            if (permission == null)
                return NotFound(new { message = $"Permission '{name}' not found" });

            return Ok(permission);
        }

        #endregion

        #region Role Permissions

        /// <summary>
        /// Get all permissions granted to a specific role
        /// </summary>
        [HttpGet("roles/{roleId}")]
        [RequiresPermission("Roles.ManagePermissions")]
        public async Task<IActionResult> GetRolePermissions(Guid roleId, [FromQuery] int? tenantId)
        {
            var rolePermissions = await _db.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
                .Select(rp => new
                {
                    rp.Id,
                    rp.PermissionName,
                    rp.IsGranted
                })
                .ToListAsync();

            return Ok(rolePermissions);
        }

        /// <summary>
        /// Grant a permission to a role
        /// </summary>
        [HttpPost("roles/{roleId}/grant")]
        [RequiresPermission("Permissions.Grant")]
        public async Task<IActionResult> GrantPermissionToRole(
            Guid roleId,
            [FromBody] PermissionRequest request)
        {
            // Check if permission exists
            var permissionExists = await _db.AsasPermission
                .AnyAsync(p => p.Name == request.PermissionName && p.TenantId == request.TenantId);

            if (!permissionExists)
                return NotFound(new { message = $"Permission '{request.PermissionName}' not found" });

            // Check if already granted
            var existing = await _db.RolePermissions
                .FirstOrDefaultAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.PermissionName == request.PermissionName &&
                    rp.TenantId == request.TenantId);

            if (existing != null)
            {
                if (existing.IsGranted)
                    return Ok(new { message = "Permission already granted to this role" });

                existing.IsGranted = true;
            }
            else
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionName = request.PermissionName,
                    IsGranted = true,
                    TenantId = request.TenantId
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Permission '{request.PermissionName}' granted to role",
                roleId,
                permissionName = request.PermissionName
            });
        }

        /// <summary>
        /// Revoke a permission from a role
        /// </summary>
        [HttpPost("roles/{roleId}/revoke")]
        [RequiresPermission("Permissions.Revoke")]
        public async Task<IActionResult> RevokePermissionFromRole(
            Guid roleId,
            [FromBody] PermissionRequest request)
        {
            var rolePermission = await _db.RolePermissions
                .FirstOrDefaultAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.PermissionName == request.PermissionName &&
                    rp.TenantId == request.TenantId);

            if (rolePermission == null)
                return NotFound(new { message = "Permission not found for this role" });

            rolePermission.IsGranted = false;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Permission '{request.PermissionName}' revoked from role",
                roleId,
                permissionName = request.PermissionName
            });
        }

        /// <summary>
        /// Grant multiple permissions to a role at once
        /// </summary>
        [HttpPost("roles/{roleId}/grant-bulk")]
        [RequiresPermission("Permissions.Grant")]
        public async Task<IActionResult> GrantMultiplePermissionsToRole(
            Guid roleId,
            [FromBody] BulkPermissionRequest request)
        {
            var results = new List<object>();

            foreach (var permissionName in request.PermissionNames)
            {
                var existing = await _db.RolePermissions
                    .FirstOrDefaultAsync(rp =>
                        rp.RoleId == roleId &&
                        rp.PermissionName == permissionName &&
                        rp.TenantId == request.TenantId);

                if (existing != null)
                {
                    existing.IsGranted = true;
                    results.Add(new { permissionName, status = "updated" });
                }
                else
                {
                    _db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionName = permissionName,
                        IsGranted = true,
                        TenantId = request.TenantId
                    });
                    results.Add(new { permissionName, status = "created" });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"{request.PermissionNames.Count} permissions processed",
                roleId,
                results
            });
        }

        #endregion

        #region User Permission Overrides

        /// <summary>
        /// Get all permission overrides for a specific user
        /// </summary>
        [HttpGet("users/{userId}")]
        [RequiresPermission("Users.ManagePermissions")]
        public async Task<IActionResult> GetUserPermissionOverrides(Guid userId, [FromQuery] int? tenantId)
        {
            var userPermissions = await _db.UserPermissionOverrides
                .Where(up => up.UserId == userId && up.TenantId == tenantId)
                .Select(up => new
                {
                    up.Id,
                    up.PermissionName,
                    up.IsGranted
                })
                .ToListAsync();

            return Ok(userPermissions);
        }

        /// <summary>
        /// Grant a permission directly to a user (override)
        /// </summary>
        [HttpPost("users/{userId}/grant")]
        [RequiresPermission("Permissions.Grant")]
        public async Task<IActionResult> GrantPermissionToUser(
            Guid userId,
            [FromBody] PermissionRequest request)
        {
            // Check if permission exists
            var permissionExists = await _db.AsasPermission
                .AnyAsync(p => p.Name == request.PermissionName && p.TenantId == request.TenantId);

            if (!permissionExists)
                return NotFound(new { message = $"Permission '{request.PermissionName}' not found" });

            // Check if already exists
            var existing = await _db.UserPermissionOverrides
                .FirstOrDefaultAsync(up =>
                    up.UserId == userId &&
                    up.PermissionName == request.PermissionName &&
                    up.TenantId == request.TenantId);

            if (existing != null)
            {
                if (existing.IsGranted == true)
                    return Ok(new { message = "Permission already granted to this user" });

                existing.IsGranted = true;
            }
            else
            {
                _db.UserPermissionOverrides.Add(new UserPermissionOverride
                {
                    UserId = userId,
                    PermissionName = request.PermissionName,
                    IsGranted = true,
                    TenantId = request.TenantId
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Permission '{request.PermissionName}' granted to user",
                userId,
                permissionName = request.PermissionName
            });
        }

        /// <summary>
        /// Revoke a permission from a user (explicit deny override)
        /// </summary>
        [HttpPost("users/{userId}/revoke")]
        [RequiresPermission("Permissions.Revoke")]
        public async Task<IActionResult> RevokePermissionFromUser(
            Guid userId,
            [FromBody] PermissionRequest request)
        {
            var existing = await _db.UserPermissionOverrides
                .FirstOrDefaultAsync(up =>
                    up.UserId == userId &&
                    up.PermissionName == request.PermissionName &&
                    up.TenantId == request.TenantId);

            if (existing != null)
            {
                existing.IsGranted = false; // Explicit deny
            }
            else
            {
                _db.UserPermissionOverrides.Add(new UserPermissionOverride
                {
                    UserId = userId,
                    PermissionName = request.PermissionName,
                    IsGranted = false, // Explicit deny
                    TenantId = request.TenantId
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Permission '{request.PermissionName}' revoked from user",
                userId,
                permissionName = request.PermissionName
            });
        }

        /// <summary>
        /// Remove a user permission override (revert to role-based)
        /// </summary>
        [HttpDelete("users/{userId}/override")]
        [RequiresPermission("Users.ManagePermissions")]
        public async Task<IActionResult> RemoveUserPermissionOverride(
            Guid userId,
            [FromQuery] string permissionName,
            [FromQuery] int? tenantId)
        {
            var override_ = await _db.UserPermissionOverrides
                .FirstOrDefaultAsync(up =>
                    up.UserId == userId &&
                    up.PermissionName == permissionName &&
                    up.TenantId == tenantId);

            if (override_ == null)
                return NotFound(new { message = "User permission override not found" });

            _db.UserPermissionOverrides.Remove(override_);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Permission override removed. User will now inherit from roles.",
                userId,
                permissionName
            });
        }

        #endregion

        #region Permission Checking

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        [HttpGet("users/{userId}/check")]
        public async Task<IActionResult> CheckUserPermission(
            Guid userId,
            [FromQuery] string permissionName,
            [FromQuery] int? tenantId)
        {
            var hasPermission = await _permissionChecker.IsGrantedAsync(
                userId,
                permissionName,
                tenantId);

            return Ok(new
            {
                userId,
                permissionName,
                hasPermission,
                tenantId
            });
        }

        /// <summary>
        /// Check multiple permissions for a user at once
        /// </summary>
        [HttpPost("users/{userId}/check-bulk")]
        public async Task<IActionResult> CheckMultiplePermissions(
            Guid userId,
            [FromBody] CheckPermissionsRequest request)
        {
            var results = new Dictionary<string, bool>();

            foreach (var permissionName in request.PermissionNames)
            {
                var hasPermission = await _permissionChecker.IsGrantedAsync(
                    userId,
                    permissionName,
                    request.TenantId);

                results[permissionName] = hasPermission;
            }

            return Ok(new
            {
                userId,
                tenantId = request.TenantId,
                permissions = results
            });
        }

        /// <summary>
        /// Get all effective permissions for a user (combines role + overrides)
        /// </summary>
        [HttpGet("users/{userId}/effective")]
        [RequiresPermission("Users.ManagePermissions")]
        public async Task<IActionResult> GetUserEffectivePermissions(
            Guid userId,
            [FromQuery] int? tenantId)
        {
            // Get all permissions
            var allPermissions = await _db.AsasPermission
                .Where(p => p.TenantId == tenantId && p.IsEnabled)
                .Select(p => p.Name)
                .ToListAsync();

            // Check each permission
            var effectivePermissions = new List<object>();

            foreach (var permissionName in allPermissions)
            {
                var hasPermission = await _permissionChecker.IsGrantedAsync(
                    userId,
                    permissionName,
                    tenantId);

                if (hasPermission)
                {
                    var permission = await _db.AsasPermission
                        .Where(p => p.Name == permissionName && p.TenantId == tenantId)
                        .Select(p => new
                        {
                            p.Name,
                            p.DisplayName,
                            p.Description,
                            p.Group
                        })
                        .FirstOrDefaultAsync();

                    effectivePermissions.Add(permission!);
                }
            }

            return Ok(new
            {
                userId,
                tenantId,
                count = effectivePermissions.Count,
                permissions = effectivePermissions
            });
        }

        #endregion
    }

    #region DTOs

    public record PermissionRequest(string PermissionName, int? TenantId);

    public record BulkPermissionRequest(List<string> PermissionNames, int? TenantId);

    public record CheckPermissionsRequest(List<string> PermissionNames, int? TenantId);

    #endregion
}
