using Asas.Core.Modularity;
using Asas.Identity.Api;                 // AsasIdentityApiModule
using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Contracts;
using Asas.Messaging.Abstractions;
using AsasKit.Api;
using AsasKit.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using AsasKit.Infrastructure;

// ---- builder ----
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication<AsasKitModule>(builder.Configuration);

// ---- app ----
var app = builder.Build();

// Auth pipeline (keep these even if module also adds them; order matters)
app.InitializeApplication();

app.MapControllers();
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
