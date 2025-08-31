// backend/Modules/Identity/AsasKit.Modules.Identity/IdentityModuleExtensions.cs
using System.Text;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Application.Services;
using Asas.Identity.Domain;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Asas.Identity.Api;

public static class IdentityModuleExtensions
{
    /// <summary>
    /// Quick-start registration that wires Identity using the default types:
    ///   - User: AsasUser
    ///   - DbContext: IdentityDbContext
    /// It also registers the non-generic AuthService (IAuthService) if present.
    /// </summary>
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration cfg,
        string? connectionString = null,
        string provider = "sqlserver")
    {
        // Reuse the generic path with default types
        services.AddIdentityModule<AsasUser, AsasIdentityDbContext<AsasUser>>(cfg, connectionString, provider);

        // If you have a non-generic AuthService, wire it here:
        // (Comment this out if you only use AuthService<TUser>.)
        services.AddScoped<IAuthService, AuthService>(); // <-- no <TUser>
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentPrincipalAccessor, HttpCurrentPrincipalAccessor>();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITokenService, TokenService>(); 

        return services;
    }

    /// <summary>
    /// Advanced registration that lets the host replace the user type and DbContext.
    /// Requires a generic AuthService&lt;TUser&gt; that implements IAuthService.
    /// </summary>
    public static IServiceCollection AddIdentityModule<TUser, TContext>(
        this IServiceCollection services,
        IConfiguration cfg,
        string? connectionString = null,
        string provider = "sqlserver")
        where TUser : AsasUser, new()
        where TContext : AsasIdentityDbContext<TUser>
    {
        // --- Connection string selection ---
        var cs = connectionString ?? cfg.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
        {
            if (string.Equals(provider, "sqlite", StringComparison.OrdinalIgnoreCase))
            {
                cs = "Data Source=identity.db"; // sensible default for sqlite
            }
            else
            {
                throw new InvalidOperationException(
                    "No connection string provided. Set ConnectionStrings:Default or pass connectionString.");
            }
        }

        // --- EF Core DbContext (provider switch) ---
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

        // --- ASP.NET Identity for TUser ---
        services.AddIdentityCore<TUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TContext>();

        // --- JWT options + validation ---
        services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
        var jwt = cfg.GetSection("Jwt").Get<JwtOptions>() ?? new();

        if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("Jwt:Key must be set and at least 32 bytes.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new()
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

        // --- IAuthService implementation ---
        // If you have AuthService<TUser> (generic), wire it here:
        return services;
    }

    /// <summary>
    /// Minimal endpoints for quick start:
    ///   POST /auth/register
    ///   POST /auth/login
    ///   POST /auth/refresh-token
    /// </summary>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/auth").WithTags("Auth");

        g.MapPost("/register", async (IAuthService svc, RegisterRequest req)
            => Results.Ok(await svc.RegisterAsync(req)));

        g.MapPost("/login", async (IAuthService svc, LoginRequest req)
            => Results.Ok(await svc.LoginAsync(req)));

        g.MapPost("/forget-password", async (IAuthService svc, ForgotPasswordRequest req)
           => Results.Ok(await svc.ForgotPasswordAsync(req)));

        g.MapPost("/refresh-token", async (ITokenService svc, RefreshRequest req)
            => Results.Ok(await svc.RefreshAsync(req)));

        return app;
    }
}
