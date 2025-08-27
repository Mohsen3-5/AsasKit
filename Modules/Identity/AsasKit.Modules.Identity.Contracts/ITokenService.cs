// AsasKit.Modules.Identity.Contracts/IRefreshTokenService.cs
namespace AsasKit.Modules.Identity.Contracts;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken, DateTime expiresAtUtc)> IssueAsync(
        Guid userId,
        Guid tenantId,
        string? displayNameOrEmail,          // used for the Name claim
        IEnumerable<string> roles,
        string? device,
        CancellationToken ct = default);

    Task<AuthResult> RefreshAsync(
        RefreshRequest req,
        CancellationToken ct = default);

    Task RevokeAsync(Guid userId, Guid tenantId, string? device = null, CancellationToken ct = default);
}
public sealed record RefreshRequest(string RefreshToken, string? Device);
