using Asas.Core.Modularity;
using AsasKit.ProjectName.Api.modularity;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication<AsasKitModule>(builder.Configuration);

var app = builder.Build();
app.InitializeApplication();


app.Run();

