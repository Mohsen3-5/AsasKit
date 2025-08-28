using AsasKit.Core.Domain;

namespace AsasKit.Modules.Identity.Events;

public sealed record UserLoggedIn(
    Guid UserId,
    Guid TenantId,
    string Email,
    string? Device
) : IAppEvent
{
    public DateTime OccurredAtUtc => DateTime.UtcNow;
}
