using MediatR;
using Microsoft.Extensions.Logging;
using AsasKit.Modules.Identity.Application.Events;

public sealed class UserLoggedInHandler : INotificationHandler<UserLoggedIn>
{
    private readonly ILogger<UserLoggedInHandler> _log;

    public UserLoggedInHandler(ILogger<UserLoggedInHandler> log) => _log = log;

    public Task Handle(UserLoggedIn e, CancellationToken ct)
    {
        // Example 1: just log
        _log.LogInformation("User logged in: {UserId} {Email} at {AtUtc} (Device: {Device})",
            e.UserId, e.Email, e.OccurredAtUtc, e.Device);
        return Task.CompletedTask;
    }
}
