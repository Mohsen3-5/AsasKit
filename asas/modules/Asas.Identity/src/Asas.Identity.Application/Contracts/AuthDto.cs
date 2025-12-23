namespace Asas.Identity.Application.Contracts
{
    public sealed record RegisterRequest(string Email, string Password);

    public sealed record LoginRequest(string Email, string Password);
    public sealed record ExternalAuthRequest(string Provider, string IdToken);

    public sealed record AuthResult(string Token, string RefreshToken, DateTime ExpiresAtUtc, bool EmailConfirmed);
    public sealed record ExternalAuthResult(string Token, string RefreshToken, DateTime ExpiresAtUtc, bool EmailConfirmed, string? Name = null, string? ProfileImageUrl = null);

    public sealed record ForgotPasswordRequest(string Email);

    public sealed record ForgotPasswordResult(bool Sent);

    public sealed record ResetPasswordRequest(string Email, string ResetToken, string NewPassword);
    public sealed record ChangePasswordRequest(string currentPassword, string NewPassword);

    public sealed record VerifyResetCodeRequest(string Email, string Code);
    public sealed record VerifyResetCodeResult(string ResetToken);


    public sealed record RegisterResult(Guid UserId, bool Created, IEnumerable<string?> Errors);

    // Logout still cares about device token (to deactivate that device)
    public sealed record LogoutRequest(
        Guid UserId,
        string? DeviceToken,
        bool AllDevices
    );

    public sealed record LogoutDto(
        string? DeviceToken,
        bool AllDevices
    );

    // ✅ New: separate device registration DTO
    public sealed record RegisterDeviceRequest(
        Guid UserId,         // or take from claims in controller and drop this
        string DeviceToken,
        string? DeviceType
    );

    public sealed record ConfirmEmailCodeRequest(string Email, string Code);
    public sealed record ResendEmailCodeRequest(string Email
        );

}
