
namespace Asas.Identity.Application.Contracts
{
    public sealed record RegisterRequest(string Email, string Password, string? DeviceToken, string? DeviceType);
    public sealed record LoginRequest(string Email, string Password, string? DeviceToken, string? DeviceType);
    public sealed record AuthResult(string Token, string RefreshToken, DateTime ExpiresAtUtc);
    public sealed record ForgotPasswordRequest(string Email);
    public sealed record ForgotPasswordResult(bool Sent, string? TokenForDevOnly);
    public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public sealed record RegisterResult(Guid UserId, bool Created);
    public sealed record LogoutRequest(
    Guid UserId,           // or get from ClaimsPrincipal in controller
    string? DeviceToken,   // FCM/APNs token of this device
    bool AllDevices        // true = log out everywhere
    );
    public sealed record LogoutDto(
    string? DeviceToken,
    bool AllDevices);

}
