// Modules/Identity/Model.cs (or a new file RefreshToken.cs next to it)
namespace AsasKit.Modules.Identity.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }

    // Store only a hash of the token (never the raw token)
    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
    public string? Device { get; set; }

    // Rotation / revocation
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByTokenHash { get; set; } // points to the new token hash when rotated

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
