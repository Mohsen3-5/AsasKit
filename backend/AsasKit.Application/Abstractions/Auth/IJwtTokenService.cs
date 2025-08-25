using System.Security.Claims;

namespace AsasKit.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string CreateToken(Guid userId, Guid tenantId, string email, IEnumerable<string> roles, IEnumerable<Claim>? extra = null);
}
