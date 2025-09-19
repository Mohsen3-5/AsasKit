using System.Text.Json;
using Asas.Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Asas.Infrastructure;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AsasException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var payload = new
            {
                error = ex.Message,
                CodeName = ex.CodeName,
                statusCode = ex.StatusCode
            };

            var json = JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync(json);
        }
    }
}
