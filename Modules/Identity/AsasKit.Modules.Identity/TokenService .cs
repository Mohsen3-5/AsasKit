// Modules/Identity/RefreshTokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AsasKit.Modules.Identity.Contracts;
using AsasKit.Modules.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AsasKit.Modules.Identity;

internal sealed class TokenService : ITokenService
{
    private readonly IdentityDbContext _db;
    private readonly UserManager<AsasUser> _userManager;
    private readonly IOptions<JwtOptions> _jwtOptions;
    // If you have it registered, swap the null helpers to use it.
    private readonly IHttpContextAccessor? _http;

    public TokenService(
        IdentityDbContext  db,
        IOptions<JwtOptions> jwtOptions,
         UserManager<AsasUser> userManager,
        IHttpContextAccessor? http = null
       )
    {
        _db = db;
        _jwtOptions = jwtOptions;
        _http = http;
        _userManager = userManager;
    }

    /// <summary>
    /// Issue an access token + refresh token for a known user.
    /// Contract passes IDs/roles to avoid referencing AsasUser in Contracts.
    /// </summary>
    public async Task<(string accessToken, string refreshToken, DateTime expiresAtUtc)> IssueAsync(
        Guid userId,
        Guid tenantId,
        string? displayNameOrEmail,
        IEnumerable<string> roles,
        string? device,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var accessToken = CreateAccessToken(userId, tenantId, displayNameOrEmail, roles, now, out var accessExpUtc);

        var rawRefresh = CreateRandomToken();             // return to caller
        var refreshHash = Hash(rawRefresh);               // store only hash
        var refreshExp = now.AddDays(_jwtOptions.Value.RefreshTokenDays);

        var rt = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            TokenHash = refreshHash,
            ExpiresAtUtc = refreshExp,
            CreatedAtUtc = now,
            CreatedByIp = GetIp(),
            UserAgent = GetUserAgent(),
            Device = device
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync(ct);

        return (accessToken, rawRefresh, accessExpUtc);
    }

    /// <summary>
    /// Validate a refresh token, rotate it, and return a new access+refresh pair.
    /// </summary>
    public async Task<AuthResult> RefreshAsync(
        RefreshRequest req,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            throw new SecurityTokenException("Refresh token required.");

        var now = DateTime.UtcNow;
        var incomingHash = Hash(req.RefreshToken);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);

        if (existing is null)
            throw new SecurityTokenException("Invalid refresh token.");

        if (existing.RevokedAtUtc is not null)
            throw new SecurityTokenException("Refresh token has been revoked.");

        if (existing.ExpiresAtUtc <= now)
            throw new SecurityTokenException("Refresh token expired.");

        if (!string.IsNullOrEmpty(req.Device) &&
            !string.Equals(req.Device, existing.Device, StringComparison.Ordinal))
            throw new SecurityTokenException("Refresh token device mismatch.");

        // Load roles + an optional display name/email for the Name claim.
        var user = await _userManager.FindByIdAsync(existing.UserId.ToString())
           ?? throw new SecurityTokenException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var displayNameOrEmail = user.UserName ?? user.Email;

        // Rotate: revoke current, create a new refresh token.
        var newRawRefresh = CreateRandomToken();
        var newHash = Hash(newRawRefresh);

        existing.RevokedAtUtc = now;
        existing.RevokedByIp = GetIp();
        existing.ReplacedByTokenHash = newHash;

        var newRt = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = existing.TenantId,
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAtUtc = now.AddDays(_jwtOptions.Value.RefreshTokenDays),
            CreatedAtUtc = now,
            CreatedByIp = GetIp(),
            UserAgent = GetUserAgent(),
            Device = req.Device
        };

        _db.RefreshTokens.Add(newRt);

        var accessToken = CreateAccessToken(
            existing.UserId,
            existing.TenantId,
            displayNameOrEmail ?? existing.UserId.ToString(),
            roles,
            now,
            out var accessExpUtc);

        await _db.SaveChangesAsync(ct);

        return new AuthResult(accessToken, newRawRefresh, accessExpUtc);
    }

    /// <summary>
    /// Revoke active refresh tokens for a user (optionally scoped by device).
    /// </summary>
    public async Task RevokeAsync(Guid userId, Guid tenantId, string? device = null, CancellationToken ct = default)
    {
        var q = _db.RefreshTokens.Where(x =>
            x.UserId == userId &&
            x.TenantId == tenantId &&
            x.RevokedAtUtc == null);

        if (!string.IsNullOrEmpty(device))
            q = q.Where(x => x.Device == device);

        var now = DateTime.UtcNow;
        await q.ForEachAsync(x =>
        {
            x.RevokedAtUtc = now;
            x.RevokedByIp = GetIp();
        }, ct);

        await _db.SaveChangesAsync(ct);
    }

    // =====================
    // Helpers
    // =====================

    private string CreateAccessToken(
        Guid userId,
        Guid tenantId,
        string? displayNameOrEmail,
        IEnumerable<string> roles,
        DateTime now,
        out DateTime expUtc)
    {
        var opts = _jwtOptions.Value;
        expUtc = now.AddMinutes(opts.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(AsasClaimTypes.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(AsasClaimTypes.TenantId, tenantId.ToString()),
            new(AsasClaimTypes.Email, displayNameOrEmail ?? string.Empty),
            new(AsasClaimTypes.PreferredUsername, displayNameOrEmail ?? string.Empty),
            new(AsasClaimTypes.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };


        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            notBefore: now,
            expires: expUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateRandomToken()
    {
        // 512-bit token → url-safe base64 (no padding)
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncoder.Encode(bytes);
    }

    private static string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes); // 64 hex chars
    }

    private string? GetIp()
    {
        var ip = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(ip) ? null : ip;
    }

    private string? GetUserAgent()
    {
        var ua = _http?.HttpContext?.Request?.Headers["User-Agent"].ToString();
        return string.IsNullOrWhiteSpace(ua) ? null : ua;
    }
}
