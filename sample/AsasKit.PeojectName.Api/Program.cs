
using Asas.Core.Modularity;
using AsasKit.ProjectName.Api.modularity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication<AsasKitModule>(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.InitializeApplication();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();
