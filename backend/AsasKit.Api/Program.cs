using Asas.Identity.Api;                 // AsasIdentityApiModule
using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Contracts;
using Asas.Messaging.Abstractions;
using Asas.Messaging.DI;
using AsasKit.Infrastructure;
using AsasKit.Infrastructure.Data;
using AsasKit.Modules.Identity;
using Microsoft.AspNetCore.Mvc;          // for [FromServices]

// ---- builder ----
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantAccessor, HeaderTenantAccessor>();

builder.Services.AddAsasKitMessaging(
    typeof(IdentityAppAssemblyMarker).Assembly,
    typeof(Program).Assembly
);

// Your infra (DbContext, UoW...). DO NOT wire Identity here; the module will.
builder.Services.AddInfrastructure(builder.Configuration);

// IMPORTANT: load the Identity module assembly (this registers Identity + JWT + DbContext, etc.)
//builder.Services.AddAsasModules(builder.Configuration, typeof(AsasIdentityApiModule).Assembly);
builder.Services.AddIdentityModule(builder.Configuration); // defaults

// Make sure controllers from the module are discoverable
builder.Services.AddControllers();

// ---- app ----
var app = builder.Build();

// Let modules run their OnApplicationInitialization (your module can call UseAuthentication/UseAuthorization too)
//app.UseAsasModules();

// Auth pipeline (keep these even if module also adds them; order matters)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapIdentityEndpoints();
app.MapGet("/health", () => Results.Ok(new { ok = true }));

// Be explicit: these come from DI
app.MapGet(
    "/me",
    async (
        [FromServices] ICurrentUser cu,
        [FromServices] IEventPublisher events,
        [FromServices] IUserDirectory userDirectory,
        CancellationToken ct
    ) =>
    {
        if (!cu.IsAuthenticated) return Results.Unauthorized();

        var me = await userDirectory.GetAsync(cu.Id!.Value, cu.TenantId!.Value, ct);

        // if (me != null) await events.PublishDomainAsync(new UserLoggedIn(...), ct);

        return me is null ? Results.NotFound() : Results.Ok(me);
    }
).RequireAuthorization();

app.Run();

// ---- local adapter ----
sealed class HeaderTenantAccessor(IHttpContextAccessor http) : ITenantAccessor
{
    public Guid CurrentTenantId =>
        Guid.TryParse(http.HttpContext?.Request.Headers["X-Tenant"], out var g) ? g : Guid.Empty;
}
