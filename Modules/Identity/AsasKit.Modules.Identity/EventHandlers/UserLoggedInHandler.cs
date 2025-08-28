// AsasKit.Modules.Identity.Application/EventHandlers/UserLoggedInHandler.cs
using AsasKit.Core.Abstractions;
using AsasKit.Modules.Identity.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AsasKit.Modules.Identity.EventHandlers;

public sealed class UserLoggedInHandler : INotificationHandler<UserLoggedIn>
{
    private readonly ILogger<UserLoggedInHandler> _log;
    public UserLoggedInHandler(ILogger<UserLoggedInHandler> log) => _log = log;

    public Task Handle(UserLoggedIn e, CancellationToken ct)
    {
        _log.LogInformation("User logged in: {UserId} {Email} at {AtUtc} (Device: {Device})",
            e.UserId, e.Email, e.OccurredAtUtc, e.Device);
        return Task.CompletedTask;
    }
}
