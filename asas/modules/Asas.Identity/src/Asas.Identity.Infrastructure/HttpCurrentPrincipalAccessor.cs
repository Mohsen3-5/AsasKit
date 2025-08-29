using System.Security.Claims;
using Asas.Identity.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Asas.Identity.Infrastructure;

public sealed class HttpCurrentPrincipalAccessor(IHttpContextAccessor accessor)
    : ICurrentPrincipalAccessor
{
    public ClaimsPrincipal? Principal => accessor.HttpContext?.User;
}
