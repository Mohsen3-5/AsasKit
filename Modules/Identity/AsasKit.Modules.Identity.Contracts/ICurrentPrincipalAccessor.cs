using System.Security.Claims;

namespace AsasKit.Modules.Identity.Contracts;

public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal? Principal { get; }
}
