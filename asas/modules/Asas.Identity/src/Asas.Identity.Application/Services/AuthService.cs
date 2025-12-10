using Asas.Core.Exceptions;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

public sealed class AuthService(
       UserManager<AsasUser> users,
       ICurrentTenant currentTenant,
       ITokenService refreshSvc,
       IEmailConfirmationCodeService codeService,
       IEmailConfirmationCodeSender codeSender,
       IOptions<AsasIdentityOptions> options,
       IUserDeviceService userDevices) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var u = new AsasUser
        {
            Email = r.Email,
            UserName = r.Email,
            TenantId = currentTenant.Id
        };

        var res = await users.CreateAsync(u, r.Password);
        if (!res.Succeeded)
        {
            var errors = res.Errors.Select(e => e.Description);
            // You probably want to surface errors later, but I’ll keep your behavior
            return new RegisterResult(Guid.Empty, Created: false,errors);
        }

        if (options.Value.RequireConfirmedEmail && !string.IsNullOrWhiteSpace(u.Email))
        {
            var code = await codeService.GenerateAndStoreAsync(u, false, ct);
            await codeSender.SendConfirmationCodeAsync(u, code, false, ct);
        }


        return new RegisterResult(u.Id, Created: true, []);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var u = await users.FindByEmailAsync(r.Email);
        if (u is null || !await users.CheckPasswordAsync(u, r.Password))
            throw AsasException.Unauthorized("Invalid email or password.");

        var roles = await users.GetRolesAsync(u);
        var auth = await IssueAsync(u, roles, ct);

        // ✅ No device token logic here anymore
        
        return auth;
    }

    private async Task<AuthResult> IssueAsync(
        AsasUser u,
        IEnumerable<string> roles,
        CancellationToken ct)
    {
        // Delegate all token issuing to RefreshTokenService
        var (access, refresh, exp) = await refreshSvc.IssueAsync(
            u.Id,
            u.UserName ?? u.Email,
            roles,
            ct);
        
        return new AuthResult(access, refresh, exp, u.EmailConfirmed);
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest r, CancellationToken ct = default)
    {
        // Don't reveal user existence for security.
        var u = await users.FindByEmailAsync(r.Email);
        if (u is null)
            return new ForgotPasswordResult(Sent: true);

        // Generate a numeric code just like email confirmation
        var code = await codeService.GenerateAndStoreAsync(u, forPasswordReset: true, ct);

        // Send the "forgot password" style email
        await codeSender.SendConfirmationCodeAsync(u, code, isForgetPasswordConfirmation: true, ct);

        return new ForgotPasswordResult(Sent: true);
    }


    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var u = await users.FindByEmailAsync(request.Email)
            ?? throw AsasException.Unauthorized("Invalid email.");

        var result = await users.ResetPasswordAsync(u, request.ResetToken, request.NewPassword);

        if (!result.Succeeded)
            throw AsasException.BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
    }


    public async Task LogoutAsync(LogoutRequest r, CancellationToken ct = default)
    {
        var u = await users.FindByIdAsync(r.UserId.ToString())
                ?? throw AsasException.Unauthorized("User not found.");

        if (r.AllDevices)
        {
            // Global logout: remove/deactivate all device tokens for this user
            await userDevices.DeactivateAllAsync(u.Id, ct);
        }
        else if (!string.IsNullOrWhiteSpace(r.DeviceToken))
        {
            // Normal logout: just this device
            await userDevices.DeactivateAsync(u.Id, r.DeviceToken!, ct);
        }
    }

    // ✅ New: separate device-token API logic
    public async Task RegisterDeviceAsync(RegisterDeviceRequest r, CancellationToken ct = default)
    {
        var u = await users.FindByIdAsync(r.UserId.ToString());
        if (u is null)
            throw AsasException.Unauthorized("User not found.");

        await userDevices.RegisterOrUpdateAsync(
            u.Id,
            r.DeviceToken,
            r.DeviceType,
            ct);
    }
}
