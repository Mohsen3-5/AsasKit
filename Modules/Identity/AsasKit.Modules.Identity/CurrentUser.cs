using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AsasKit.Modules.Identity.Contracts;

namespace AsasKit.Modules.Identity;

internal sealed class CurrentUser(ICurrentPrincipalAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal P => accessor.Principal ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated => P.Identity?.IsAuthenticated ?? false;

    public Guid? Id =>
        TryGuid(
            // mapped sub -> nameidentifier
            P.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? P.FindFirstValue(AsasClaimTypes.Sub)
        );

    public Guid? TenantId =>
        TryGuid(
            // MS mapped tenant claim
            P.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid")
            ?? P.FindFirstValue(AsasClaimTypes.TenantId)
        );

    public string? UserName =>
        P.FindFirstValue(AsasClaimTypes.PreferredUsername)
        ?? P.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        P.FindFirstValue(ClaimTypes.Email)
        ?? P.FindFirstValue(AsasClaimTypes.Email);

    public string? PhoneNumber =>
        P.FindFirstValue(ClaimTypes.MobilePhone)
        ?? P.FindFirstValue(AsasClaimTypes.PhoneNumber);

    public IReadOnlyList<string> Roles =>
        P.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

    public Guid? ImpersonatorUserId =>
        TryGuid(P.FindFirstValue(AsasClaimTypes.ImpersonatorUserId));

    public Guid? ImpersonatorTenantId =>
        TryGuid(P.FindFirstValue(AsasClaimTypes.ImpersonatorTenantId));

    public Claim? FindClaim(string type) => P.Claims.FirstOrDefault(c => c.Type == type);
    public Claim[] FindClaims(string type) => P.Claims.Where(c => c.Type == type).ToArray();
    public Claim[] GetAllClaims() => P.Claims.ToArray();

    public bool IsInRole(string roleName) =>
        !string.IsNullOrWhiteSpace(roleName) &&
        P.IsInRole(roleName);

    static Guid? TryGuid(string? s) => Guid.TryParse(s, out var g) ? g : null;
}
