using Asas.Core.Exceptions;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Identity;

public sealed class AuthService(
       UserManager<AsasUser> users,
       ICurrentTenant currentTenant,
       ITokenService refreshSvc) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var u = new AsasUser { Email = r.Email, UserName = r.Email, TenantId = currentTenant.Id, DeviceToken = r.DeviceToken };
        var res = await users.CreateAsync(u, r.Password);
        if (!res.Succeeded)
        {
            var errors = res.Errors.Select(e => e.Description);
            return new RegisterResult(Guid.Empty, Created: false);
        }

        return new RegisterResult(u.Id, Created: true);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var u = await users.FindByEmailAsync(r.Email);
        if (u is null || !await users.CheckPasswordAsync(u, r.Password))
            throw  AsasException.Unauthorized("Invalid email or password.");

        var roles = await users.GetRolesAsync(u);
        var auth = await IssueAsync(u, roles, ct);
        if(!string.IsNullOrEmpty(r.DeviceToken) && r.DeviceToken != u.DeviceToken && !string.IsNullOrEmpty(auth.Token))
        {
            u.DeviceToken = r.DeviceToken;
            await users.UpdateAsync(u);
        }

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

        return new AuthResult(access, refresh, exp);
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest r, CancellationToken ct = default)
    {
        // Don’t reveal whether the email exists (user enumeration hardening).
        var u = await users.FindByEmailAsync(r.Email);
        if (u is null)
            return new ForgotPasswordResult(Sent: true, TokenForDevOnly: null);

        var token = await users.GeneratePasswordResetTokenAsync(u);

        // Send email (or store in logs/dev response)
        try
        {
            //TODO ADD EMAIL FUNCINALITY
            //var subject = "Reset your password";
            //var body = $"""
            //            Use this code to reset your password:
            //            <pre>{System.Net.WebUtility.HtmlEncode(token)}</pre>
            //            If you didn’t request this, ignore this email.
            //            """;
            //await email.SendAsync(r.Email, subject, body, ct);
            return new ForgotPasswordResult(Sent: true, TokenForDevOnly: null);
        }
        catch
        {
            // In dev you may want to surface the token even if email fails.
#if DEBUG
            return new ForgotPasswordResult(Sent: false,
                                            TokenForDevOnly: token);
#else
                        throw;
#endif
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var u = await users.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException();

        var res = await users.ResetPasswordAsync(u, request.Token, request.NewPassword);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));
    }
}
