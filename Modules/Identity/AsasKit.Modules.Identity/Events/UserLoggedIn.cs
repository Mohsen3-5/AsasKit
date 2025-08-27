using AsasKit.Application;

namespace AsasKit.Modules.Identity.Application.Events;

public sealed record UserLoggedIn(
    Guid UserId,
    Guid TenantId,
    string Email,
    string? Device
) : AppEventBase;