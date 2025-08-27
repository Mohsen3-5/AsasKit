namespace AsasKit.Modules.Identity.Contracts;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}

public sealed record RegisterRequest(string Email, string Password, Guid TenantId , string? Device);
public sealed record LoginRequest(string Email, string Password, string? Device);
public sealed record AuthResult(string Token, string RefreshToken, DateTime ExpiresAtUtc);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ForgotPasswordResult(bool Sent, string? TokenForDevOnly);
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);