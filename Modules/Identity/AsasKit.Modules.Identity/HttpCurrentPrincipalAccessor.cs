using System.Security.Claims;
using AsasKit.Modules.Identity.Contracts;
using Microsoft.AspNetCore.Http;

namespace AsasKit.Modules.Identity;

internal sealed class HttpCurrentPrincipalAccessor(IHttpContextAccessor accessor)
    : ICurrentPrincipalAccessor
{
    public ClaimsPrincipal? Principal => accessor.HttpContext?.User;
}
