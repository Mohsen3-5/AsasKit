using System.Security.Claims;

namespace Asas.Identity.Infrastructure;

public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal? Principal { get; }
}
