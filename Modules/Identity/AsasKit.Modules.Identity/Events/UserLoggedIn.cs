using AsasKit.Core;
using MediatR;

namespace AsasKit.Modules.Identity.Application.Events;

public sealed record UserLoggedIn(
    Guid UserId,
    Guid TenantId,
    string Email,
    string? Device
) : IAppEvent, INotification
{
    public DateTime OccurredAtUtc => DateTime.UtcNow;
}
