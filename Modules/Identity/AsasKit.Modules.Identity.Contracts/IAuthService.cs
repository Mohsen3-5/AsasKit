namespace AsasKit.Modules.Identity.Contracts;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public sealed record RegisterRequest(string Email, string Password, Guid TenantId);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResult(string Token, DateTime ExpiresAtUtc);
