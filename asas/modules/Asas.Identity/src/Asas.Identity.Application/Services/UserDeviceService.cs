using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Asas.Identity.Application.Services;

public class UserDeviceService : IUserDeviceService
{
    private readonly AsasIdentityDbContext _db;

    public UserDeviceService(AsasIdentityDbContext db)
    {
        _db = db;
    }

    public async Task RegisterOrUpdateAsync(
        Guid userId,
        string deviceToken,
        string? deviceType,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            return;

        var now = DateTime.UtcNow;

        var existing = await _db.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == deviceToken, ct);

        if (existing is null)
        {
            var d = new UserDevice
            {
                UserId = userId,
                DeviceToken = deviceToken,
                DeviceType = deviceType ?? "Unknown",
                CreatedAtUtc = now,
                IsActive = true
            };

            _db.UserDevices.Add(d);
        }
        else
        {
            existing.IsActive = true;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(
       Guid userId,
       string deviceToken,
       CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            return;

        var devices = await _db.UserDevices
            .Where(d => d.UserId == userId && d.DeviceToken == deviceToken && d.IsActive)
            .ToListAsync(ct);

        if (!devices.Any())
            return;

        foreach (var d in devices)
        {
            d.IsActive = false;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeactivateAllAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var devices = await _db.UserDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(ct);

        if (!devices.Any())
            return;

        foreach (var d in devices)
        {
            d.IsActive = false;
        }

        await _db.SaveChangesAsync(ct);
    }
}
