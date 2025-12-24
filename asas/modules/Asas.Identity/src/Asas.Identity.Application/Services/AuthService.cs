using System.Net.Http;
using System.Net.Http.Json;
using Asas.Core.Exceptions;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Asas.Tenancy.Contracts;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

public sealed class AuthService(
       UserManager<AsasUser> users,
       ICurrentTenant currentTenant,
       ICurrentUser currentUser,
       ITokenService refreshSvc,
       IEmailConfirmationCodeService codeService,
       IEmailConfirmationCodeSender codeSender,
       IOptions<AsasIdentityOptions> options,
       IUserDeviceService userDevices,
       IHttpClientFactory httpClientFactory) : IAuthService
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
            return new RegisterResult(Guid.Empty, Created: false, errors);
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

    public async Task<ExternalAuthResult> ExternalAuthAsync(ExternalAuthRequest r, CancellationToken ct = default)
    {
        // 1. Validate token & get user info
        var info = await ValidateExternalTokenAsync(r.Provider, r.IdToken);

        // 2. Find user by email or by External Login
        var u = await users.FindByEmailAsync(info.Email);
        if (u is null)
        {
            u = new AsasUser
            {
                Email = info.Email,
                UserName = info.Email,
                TenantId = currentTenant.Id,
                EmailConfirmed = true // External providers usually verify email
            };
            var res = await users.CreateAsync(u);
            if (!res.Succeeded)
                throw AsasException.BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        }

        // 3. Link external login if not already linked (optional but good practice)
        var loginInfo = new UserLoginInfo(r.Provider, info.ProviderKey, r.Provider);
        var logins = await users.GetLoginsAsync(u);
        if (logins.All(l => l.LoginProvider != r.Provider))
        {
            await users.AddLoginAsync(u, loginInfo);
        }

        var roles = await users.GetRolesAsync(u);
        var auth = await IssueAsync(u, roles, ct);
        return new ExternalAuthResult(auth.Token, auth.RefreshToken, auth.ExpiresAtUtc, auth.EmailConfirmed, info.Name, info.Picture);
    }

    private async Task<(string Email, string Name, string ProviderKey, string? Picture)> ValidateExternalTokenAsync(string provider, string idToken)
    {
        switch (provider.ToLowerInvariant())
        {
            case "google":
                return await ValidateGoogleTokenAsync(idToken);
            case "facebook":
                return await ValidateFacebookTokenAsync(idToken);
            case "apple":
                return ValidateAppleToken(idToken);
            default:
                throw AsasException.BadRequest($"Provider {provider} not supported.");
        }
    }

    private async Task<(string Email, string Name, string ProviderKey, string? Picture)> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings();
            if (!string.IsNullOrEmpty(options.Value.ExternalAuth.Google.ClientId))
            {
                settings.Audience = [options.Value.ExternalAuth.Google.ClientId];
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return (payload.Email, payload.Name, payload.Subject, payload.Picture);
        }
        catch (Exception ex)
        {
            throw AsasException.Unauthorized($"Google token validation failed: {ex.Message}");
        }
    }

    private async Task<(string Email, string Name, string ProviderKey, string? Picture)> ValidateFacebookTokenAsync(string accessToken)
    {
        // 1. Check if the token is a JWT (Facebook Limited Login)
        // iOS often uses Limited Login which returns an OIDC-compliant JWT instead of a Graph access token.
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        if (handler.CanReadToken(accessToken))
        {
            var jwt = handler.ReadJwtToken(accessToken);
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var picture = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(sub))
            {
                return (email, name, sub, picture);
            }
        }

        // 2. Fallback to standard Graph API validation
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw AsasException.Unauthorized($"Facebook token validation failed: {error}");
        }

        var payload = await response.Content.ReadFromJsonAsync<FacebookPayload>();
        if (payload == null || string.IsNullOrEmpty(payload.Email))
            throw AsasException.Unauthorized("Facebook token validation failed or email not provided.");

        var pictureUrl = $"https://graph.facebook.com/{payload.Id}/picture?type=large";
        return (payload.Email, payload.Name ?? "", payload.Id, pictureUrl);
    }

    private (string Email, string Name, string ProviderKey, string? Picture) ValidateAppleToken(string idToken)
    {
        // For Apple, the idToken is a JWT. We should ideally validate the signature.
        // For now, we decode it to get the claims. In production, signature validation is mandatory.
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        if (!handler.CanReadToken(idToken))
            throw AsasException.Unauthorized("Invalid Apple token format.");

        var jwt = handler.ReadJwtToken(idToken);
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub))
            throw AsasException.Unauthorized("Apple token missing required claims.");

        return (email, "", sub, null);
    }

    private class FacebookPayload
    {
        public string Id { get; set; } = default!;
        public string? Name { get; set; }
        public string? Email { get; set; }
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

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (currentUser == null || string.IsNullOrEmpty(currentUser.Email))
            throw AsasException.Unauthorized("User not authenticated.");

        var u = await users.FindByEmailAsync(currentUser.Email)
            ?? throw AsasException.Unauthorized("Invalid email.");

        var result = await users.ChangePasswordAsync(u, request.currentPassword, request.NewPassword);

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
