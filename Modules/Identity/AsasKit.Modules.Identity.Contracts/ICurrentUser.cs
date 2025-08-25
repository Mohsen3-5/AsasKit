using System.Security.Claims;

namespace AsasKit.Modules.Identity.Contracts;

/// ABP-like current user abstraction
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? Id { get; }                 // user id (null if anonymous)
    Guid? TenantId { get; }           // tenant id (supports mapped tid)

    // Optional identity info (null if missing)
    string? UserName { get; }
    string? Email { get; }
    string? PhoneNumber { get; }

    IReadOnlyList<string> Roles { get; }

    // Impersonation slots (null unless you add that feature)
    Guid? ImpersonatorUserId { get; }
    Guid? ImpersonatorTenantId { get; }

    // Claims helpers
    Claim? FindClaim(string claimType);
    Claim[] FindClaims(string claimType);
    Claim[] GetAllClaims();

    bool IsInRole(string roleName);
}
