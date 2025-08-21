using AsasKit.Application.Abstractions;
using AsasKit.Application.Tenancy;    
using AsasKit.Application.Behaviors;
using AsasKit.Infrastructure;
using AsasKit.Infrastructure.Data;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// HttpContext accessor for providers
builder.Services.AddHttpContextAccessor();

// Infra (EF/Identity/etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR + Validators
builder.Services.AddMediatR(cfg =>
                            {
                                cfg.RegisterServicesFromAssemblyContaining<CreateTenant>();
                            });
builder.Services.AddValidatorsFromAssembly(typeof(CreateTenant).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Tenant & current user providers (HTTP-scoped)
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

// Map infra tenant accessor to app tenant provider
builder.Services.AddScoped<ITenantAccessor>(sp =>
{
    var tp = sp.GetRequiredService<ITenantProvider>();
    return new TenantAccessor(tp.CurrentTenantId);
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { ok = true, ts = DateTime.UtcNow }));

app.Run();

/// ------------ HTTP adapters (Api-layer only) ------------
sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId => TryGuid(accessor.HttpContext?.User?.FindFirst("sub")?.Value);
    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    static Guid TryGuid(string? s) => Guid.TryParse(s, out var g) ? g : Guid.Empty;
}

sealed class HttpTenantProvider(IHttpContextAccessor accessor) : ITenantProvider
{
    // For now: from header; later switch to subdomain resolver
    public Guid CurrentTenantId =>
        Guid.TryParse(accessor.HttpContext?.Request.Headers["X-Tenant"], out var g) ? g : Guid.Empty;
}
