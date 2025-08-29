using System.Security.Claims;

namespace Asas.Identity.Application.Contracts;

public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal? Principal { get; }
}
