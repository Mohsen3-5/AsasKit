using AsasKit.Core.Abstractions;
using AsasKit.Infrastructure;
using AsasKit.Infrastructure.Data;
using AsasKit.Modules.Identity;
using AsasKit.Modules.Identity.Contracts;
using AsasKit.Modules.Identity.Events;
using Kernel;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Provide tenant info to AppDbContext (reads header X-Tenant)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantAccessor, HeaderTenantAccessor>();
builder.Services.AddAsasKitMessaging(
    typeof(IdentityAppAssemblyMarker).Assembly,
    typeof(Program).Assembly
);
// Use Infra (DbContext, repos, UoW...) but DO NOT register Identity here.
// The Identity module will own Identity.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Boot modules (IdentityStartupModule calls AddIdentityModule + maps /auth)
var app = ModuleRunner.Build(builder, typeof(IdentityStartupModule));

app.MapGet("/health", () => Results.Ok(new { ok = true }));
app.MapGet("/me", async (ICurrentUser cu,IEventPublisher events, IUserDirectory userDirectory, CancellationToken cancellationToken) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();

    var me = await userDirectory.GetAsync(cu.Id!.Value, cu.TenantId!.Value, cancellationToken); // ✅ await

    if(me != null)
    {
        await events.PublishDomainAsync(new UserLoggedIn(
            UserId: me.Id,
            TenantId: me.TenantId,
            Email: me.Email ?? "",
            Device: ""
        ), cancellationToken);
    }
 
    return me is null ? Results.NotFound() : Results.Ok(me);
}).RequireAuthorization();


app.Run();

// ---- local adapter ----
sealed class HeaderTenantAccessor(IHttpContextAccessor http) : ITenantAccessor
{
    public Guid CurrentTenantId =>
        Guid.TryParse(http.HttpContext?.Request.Headers["X-Tenant"], out var g) ? g : Guid.Empty;
}

