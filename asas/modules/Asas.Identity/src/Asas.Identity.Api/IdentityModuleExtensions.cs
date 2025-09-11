// backend/Modules/Identity/AsasKit.Modules.Identity/IdentityModuleExtensions.cs
using System.Text;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Application.Services;
using Asas.Identity.Domain;
using Asas.Identity.Domain.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Asas.Identity.Infrastructure.Repo;
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
using Microsoft.IdentityModel.Tokens;

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

        // Token service
        services.TryAddScoped<ITokenService, TokenService>();

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
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TContext>();

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

        // ----- Generic AuthService<TUser> → IAuthService -----
        // If you also registered the non-generic above, the first registration "wins".
        services.TryAddScoped<IAuthService, AuthService>();

        // Current user plumbing (ensure present in generic overload too)
        services.TryAddScoped<ICurrentPrincipalAccessor, HttpCurrentPrincipalAccessor>();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IUserRoleService, UserRoleService>();

        // Token service
        services.TryAddScoped<ITokenService, TokenService>();

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
         .WithName("Auth_Register");

        g.MapPost("/login", async ([FromServices] IAuthService svc, [FromBody] LoginRequest req, CancellationToken ct) =>
            Results.Ok(await svc.LoginAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_Login");

        g.MapPost("/forget-password", async ([FromServices] IAuthService svc, [FromBody] ForgotPasswordRequest req, CancellationToken ct) =>
            Results.Ok(await svc.ForgotPasswordAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_ForgotPassword");

        g.MapPost("/refresh-token", async ([FromServices] ITokenService svc, [FromBody] RefreshRequest req, CancellationToken ct) =>
            Results.Ok(await svc.RefreshAsync(req, ct)))
         .AllowAnonymous()
         .WithName("Auth_RefreshToken");

        return app;
    }
}
