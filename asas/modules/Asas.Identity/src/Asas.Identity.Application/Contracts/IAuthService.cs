namespace Asas.Identity.Application.Contracts
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}
