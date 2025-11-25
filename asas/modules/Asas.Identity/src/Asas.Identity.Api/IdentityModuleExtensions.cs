// backend/Modules/Identity/AsasKit.Modules.Identity/IdentityModuleExtensions.cs
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Text;
using Asas.Identity.Application;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Application.Services;
using Asas.Identity.Domain;
using Asas.Identity.Domain.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Asas.Identity.Infrastructure.Repo;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static System.Net.WebRequestMethods;

namespace Asas.Identity.Api;

public static class IdentityModuleExtensions
{
    /// <summary>
    /// Quick-start registration using defaults:
    ///   TUser = AsasUser, DbContext = AsasIdentityDbContext&lt;AsasUser&gt;.
    /// Wires JWT, Identity, CurrentUser plumbing, TokenService, and (optionally)
    /// a non-generic IAuthService implementation if you have one.
    /// </summary>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration cfg,
        string? connectionString = null,
        string provider = "sqlserver")
    {
        services.AddIdentityModule<AsasUser, AsasIdentityDbContext>(cfg, connectionString, provider);

        // Current user plumbing
        services.TryAddScoped<ICurrentPrincipalAccessor, HttpCurrentPrincipalAccessor>();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.AddHostedService<IdentitySynchronizer>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.TryAddScoped<IEmailConfirmationCodeSender, NullEmailConfirmationCodeSender>();

        // Token service
        services.TryAddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.Configure<AsasIdentityOptions>(cfg.GetSection("AsasIdentity"));
        services.AddScoped<IEmailConfirmationCodeService, EmailConfirmationCodeService>();

        // If you have a non-generic AuthService that implements IAuthService, wire it.
        // If you only have AuthService<TUser>, comment this line (generic is registered below).
        services.TryAddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// Advanced registration where host can swap the user type and DbContext.
    /// Requires AuthService&lt;TUser&gt; implementing IAuthService.
    /// </summary>
    public static IServiceCollection AddIdentityModule<TUser, TContext>(
        this IServiceCollection services,
        IConfiguration cfg,
        string? connectionString = null,
        string provider = "sqlserver")
        where TUser : AsasUser, new()
        where TContext : AsasIdentityDbContext
    {
        using var sp = services.BuildServiceProvider();

        // ----- Connection string -----
        var cs = connectionString ?? cfg.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
        {
            if (string.Equals(provider, "sqlite", StringComparison.OrdinalIgnoreCase))
            {
                cs = "Data Source=identity.db";
            }
            else
            {
                throw new InvalidOperationException(
                    "No connection string provided. Set ConnectionStrings:Default or pass connectionString.");
            }
        }

        // ----- DbContext + migrations assembly -----
        services.AddDbContext<TContext>(b =>
        {
            var migAsm = typeof(TContext).Assembly.FullName;
            switch ((provider ?? "sqlserver").Trim().ToLowerInvariant())
            {
                case "postgres":
                case "postgresql":
                    b.UseNpgsql(cs, x => x.MigrationsAssembly(migAsm));
                    break;
                case "sqlite":
                    b.UseSqlite(cs, x => x.MigrationsAssembly(migAsm));
                    break;
                default: // sqlserver
                    b.UseSqlServer(cs, x => x.MigrationsAssembly(migAsm));
                    break;
            }
        });

        // ----- ASP.NET Identity -----
        services.AddIdentityCore<TUser>(opts =>
        {
            // sensible defaults; host can override via PostConfigure<IdentityOptions>
            opts.User.RequireUniqueEmail = true;
            opts.Password.RequireDigit = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Password.RequiredLength = 6;
        })
            .AddRoles<AsasRole>()
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders();

        // ----- JWT binding (Auth:Jwt preferred, fallback to Jwt) -----
        var jwtSection = cfg.GetSection("Auth:Jwt");
        if (!jwtSection.Exists())
            jwtSection = cfg.GetSection("Jwt");

        services.Configure<JwtOptions>(jwtSection);
        var jwt = jwtSection.Get<JwtOptions>() ?? new();

        if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("Jwt:Key must be set and at least 32 bytes long.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(); // host can still add policies later

        return services;
    }

    /// <summary>
    /// Minimal Auth endpoints:
    ///   POST /auth/register
    ///   POST /auth/login
    ///   POST /auth/forget-password
    ///   POST /auth/refresh-token
    /// </summary>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/auth").WithTags("Auth");

        g.MapPost("/register", async ([FromServices] IAuthService svc, [FromBody] RegisterRequest req, CancellationToken ct) =>
            Results.Ok(await svc.RegisterAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_Register")
         .Produces<RegisterResult>(StatusCodes.Status200OK);

        g.MapPost("/login", async ([FromServices] IAuthService svc, [FromBody] LoginRequest req, CancellationToken ct) =>
            Results.Ok(await svc.LoginAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_Login")
         .Produces<AuthResult>(StatusCodes.Status200OK)
         .Produces(StatusCodes.Status401Unauthorized);


        g.MapPost("/forget-password", async ([FromServices] IAuthService svc, [FromBody] ForgotPasswordRequest req, CancellationToken ct) =>
            Results.Ok(await svc.ForgotPasswordAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_ForgotPassword")
          .Produces<ForgotPasswordResult>(StatusCodes.Status200OK);


        g.MapPost("/reset-password", async (
          [FromServices] IAuthService svc,
          [FromBody] ResetPasswordRequest req,
          CancellationToken ct) =>
        {
            await svc.ResetPasswordAsync(req, ct);
            return Results.NoContent();
        })
          .AllowAnonymous()
          .WithName("Auth_ResetPassword")
          .Produces(StatusCodes.Status204NoContent);


        g.MapPost("/verify-reset-code", async (
        [FromServices] UserManager<AsasUser> userManager,
        [FromServices] IEmailConfirmationCodeService codeService,
        [FromBody] VerifyResetCodeRequest req,
        CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Results.BadRequest("Invalid email or code.");

            // Validate 6-digit code using your service
            var ok = await codeService.VerifyAsync(user.Id, req.Code, true, ct);
            if (!ok)
                return Results.BadRequest("Invalid or expired code.");

            // Now generate Identity reset token (long token, secure)
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            return Results.Ok(new VerifyResetCodeResult(resetToken));
        })
        .AllowAnonymous()
        .WithName("Auth_VerifyResetCode")
        .Produces<VerifyResetCodeResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);


        g.MapPost("/refresh-token", async ([FromServices] ITokenService svc, [FromBody] RefreshRequest req, CancellationToken ct) =>
            Results.Ok(await svc.RefreshAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_RefreshToken")
         .Produces<AuthResult>(StatusCodes.Status200OK);

        g.MapPost("/logout", async (
               HttpContext http,
               [FromServices] IAuthService svc,
               [FromBody] LogoutDto req,
               CancellationToken ct) =>
        {
            // Get userId from JWT (NameIdentifier or "sub")
            var userIdClaim =
                http.User.FindFirst(ClaimTypes.NameIdentifier) ??
                http.User.FindFirst("sub");

            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var logoutReq = new LogoutRequest(
                userId,
                req.DeviceToken,
                req.AllDevices);

            await svc.LogoutAsync(logoutReq, ct);

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Auth_Logout")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized);


        g.MapPost("/device", async (
              HttpContext http,
              [FromServices] IAuthService svc,
              [FromBody] RegisterDeviceRequest req,
              CancellationToken ct) =>
        {
            var userIdClaim =
                http.User.FindFirst(ClaimTypes.NameIdentifier) ??
                http.User.FindFirst("sub");

            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            // Never trust userId from body – override it
            var cmd = req with { UserId = userId };

            await svc.RegisterDeviceAsync(cmd, ct);

            return Results.NoContent();
        })
          .RequireAuthorization()
          .WithName("Auth_RegisterDevice")
          .Produces(StatusCodes.Status204NoContent)
          .Produces(StatusCodes.Status401Unauthorized);

        g.MapPost("/confirm-email", async (
          [FromServices] UserManager<AsasUser> userManager,
          [FromServices] IEmailConfirmationCodeService codeService,
          [FromBody] ConfirmEmailCodeRequest dto,
          CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Results.NotFound("User not found.");

            var ok = await codeService.VerifyAsync(user.Id, dto.Code, false, ct);
            if (!ok)
                return Results.BadRequest("Invalid or expired confirmation code.");

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return Results.Problem("Failed to update user.");
            }

            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithName("Auth_ConfirmEmail")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);


        // ✅ RESEND EMAIL CODE
        g.MapPost("/resend-email-code", async (
                [FromServices] UserManager<AsasUser> userManager,
                [FromServices] IEmailConfirmationCodeService codeService,
                [FromServices] IEmailConfirmationCodeSender codeSender,
                [FromServices] IOptions<AsasIdentityOptions> options,
                [FromBody] ResendEmailCodeRequest dto,
                CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Results.NotFound("User not found.");

            var opt = options.Value;

            if (!opt.RequireConfirmedEmail)
                return Results.BadRequest("Email confirmation is disabled.");

            if (user.EmailConfirmed)
                return Results.BadRequest("Email is already confirmed.");

            var code = await codeService.GenerateAndStoreAsync(user,false , ct);
            await codeSender.SendConfirmationCodeAsync(user, code, false, ct);

            return Results.NoContent();
        })
          .AllowAnonymous()
          .WithName("Auth_ResendEmailCode")
          .Produces(StatusCodes.Status204NoContent)
          .Produces(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status404NotFound);

        return g;
    }
}
