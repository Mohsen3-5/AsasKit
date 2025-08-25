using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AsasKit.Modules.Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AsasKit.Modules.Identity;

internal sealed class AuthService(
    UserManager<AsasUser> users,
    IOptions<JwtOptions> jwtOpt) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOpt.Value;

    public async Task<AuthResult> RegisterAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var u = new AsasUser { Email = r.Email, UserName = r.Email, TenantId = r.TenantId };
        var res = await users.CreateAsync(u, r.Password);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
        return Issue(u, roles: Array.Empty<string>());
    }

    public async Task<AuthResult> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var u = await users.FindByEmailAsync(r.Email);
        if (u is null || !await users.CheckPasswordAsync(u, r.Password))
            throw new UnauthorizedAccessException();
        var roles = await users.GetRolesAsync(u);
        return Issue(u, roles);
    }

    private AuthResult Issue(AsasUser u, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var exp = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
{
            new(AsasClaimTypes.Sub, u.Id.ToString()),
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(AsasClaimTypes.TenantId, u.TenantId.ToString()),
            new(AsasClaimTypes.Email, u.Email ?? ""),
            new(AsasClaimTypes.PreferredUsername, u.UserName ?? (u.Email ?? "")),
            new(AsasClaimTypes.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims,
            notBefore: DateTime.UtcNow, expires: exp, signingCredentials: creds);

        return new AuthResult(new JwtSecurityTokenHandler().WriteToken(token), exp);
    }
}
