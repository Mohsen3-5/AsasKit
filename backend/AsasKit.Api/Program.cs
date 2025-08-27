using AsasKit.Modules.;
using AsasKit.Core;
using AsasKit.Infrastructure;
using AsasKit.Infrastructure.Data;
using AsasKit.Kernel;
using AsasKit.Modules.Identity;
using AsasKit.Modules.Identity.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Provide tenant info to AppDbContext (reads header X-Tenant)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantAccessor, HeaderTenantAccessor>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(IdentityAppAssemblyMarker).Assembly,
        typeof(Program).Assembly);
});
builder.Services.AddScoped<IEventPublisher, MediatREventPublisher>();
// Use Infra (DbContext, repos, UoW...) but DO NOT register Identity here.
// The Identity module will own Identity.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Boot modules (IdentityStartupModule calls AddIdentityModule + maps /auth)
var app = ModuleRunner.Build(builder, typeof(IdentityStartupModule));

app.MapGet("/health", () => Results.Ok(new { ok = true }));
app.MapGet("/me", async (ICurrentUser cu, IUserDirectory userDirectory, CancellationToken cancellationToken) =>
{
    if (!cu.IsAuthenticated) return Results.Unauthorized();

    var me = await userDirectory.GetAsync(cu.Id!.Value, cu.TenantId!.Value, cancellationToken); // ✅ await
    return me is null ? Results.NotFound() : Results.Ok(me);
}).RequireAuthorization();


app.Run();

// ---- local adapter ----
sealed class HeaderTenantAccessor(IHttpContextAccessor http) : ITenantAccessor
{
    public Guid CurrentTenantId =>
        Guid.TryParse(http.HttpContext?.Request.Headers["X-Tenant"], out var g) ? g : Guid.Empty;
}

