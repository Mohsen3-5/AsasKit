using Asas.Core.Modularity;
using Asas.Infrastructure.Repositories;
using AsasKit.Api;
using AsasKit.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Asas Kit modularity
builder.Services.AddApplication<AsasKitModule>(builder.Configuration);

// Add basic services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AsasKit API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(o =>
{
    var provider = builder.Configuration["Data:Provider"]?.ToLowerInvariant() ?? "postgresql";
    var connectionString = builder.Configuration.GetConnectionString("Default");

    switch (provider)
    {
        case "postgresql":
        case "postgres":
        case "npgsql":
            o.UseNpgsql(connectionString);
            break;
        case "sqlserver":
        case "mssql":
            o.UseSqlServer(connectionString);
            break;
        case "sqlite":
            o.UseSqlite(connectionString);
            break;
        default:
            throw new InvalidOperationException($"Unsupported database provider: {provider}");
    }
});

var app = builder.Build();

// Run migrations (optional, but helpful for a starter kit)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Initialize Asas Kit modules
app.InitializeApplication();

// Use Asas Kit infrastructure (Tenancy, Identity middleware etc.)
app.UseAsasInfrastructure();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
