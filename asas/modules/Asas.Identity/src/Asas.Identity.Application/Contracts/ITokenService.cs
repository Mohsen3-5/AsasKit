// AsasKit.Modules.Identity.Contracts/IRefreshTokenService.cs
namespace Asas.Identity.Application.Contracts;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken, DateTime expiresAtUtc)> IssueAsync(
        Guid userId,
        string? displayNameOrEmail,          // used for the Name claim
        IEnumerable<string> roles,
        CancellationToken ct = default);

    Task<AuthResult> RefreshAsync(
        RefreshRequest req,
        CancellationToken ct = default);

    Task RevokeAsync(Guid userId, int tenantId, string? device = null, CancellationToken ct = default);
}
public sealed record RefreshRequest(string RefreshToken, string? Device);
