
namespace Asas.Identity.Application.Contracts;
public interface IUserDeviceService
{
    Task RegisterOrUpdateAsync(
        Guid userId,
        string deviceToken,
        string? deviceType,
        CancellationToken ct = default);

    // Logout from ONE device (normal case: user logs out from this phone)
    Task DeactivateAsync(
        Guid userId,
        string deviceToken,
        CancellationToken ct = default);

    // Logout from ALL devices (e.g. security reason, reset)
    Task DeactivateAllAsync(
        Guid userId,
        CancellationToken ct = default);
}
