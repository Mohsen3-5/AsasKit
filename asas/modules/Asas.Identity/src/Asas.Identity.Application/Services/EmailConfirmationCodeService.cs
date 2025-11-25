using System.Security.Cryptography;
using Asas.Identity.Application.Contracts;
using Asas.Identity.Domain.Entities;
using Asas.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Asas.Identity.Application.Services;


public class EmailConfirmationCodeService : IEmailConfirmationCodeService
{
    private readonly AsasIdentityDbContext _db;
    private readonly AsasIdentityOptions _options;

    public EmailConfirmationCodeService(
        AsasIdentityDbContext db,
        IOptions<AsasIdentityOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<string> GenerateAndStoreAsync(AsasUser user, bool forPasswordReset = false, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var purpose = forPasswordReset ? "Password" : "Email";

        // Remove old codes ONLY for this purpose
        var oldCodes = _db.EmailConfirmationCodes
            .Where(x => x.UserId == user.Id && x.Purpose == purpose && !x.Used);

        _db.EmailConfirmationCodes.RemoveRange(oldCodes);

        var code = GenerateNumericCode(6);

        var entity = new EmailConfirmationCode
        {
            UserId = user.Id,
            Code = code,
            Purpose = purpose,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(_options.EmailConfirmationCodeTtlMinutes),
            Attempts = 0,
            Used = false,
        };

        _db.EmailConfirmationCodes.Add(entity);
        await _db.SaveChangesAsync(ct);

        return code;
    }


    public async Task<bool> VerifyAsync(Guid userId, string code, bool forPasswordReset = false, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var purpose = forPasswordReset ? "Password" : "Email";

        var entity = await _db.EmailConfirmationCodes
            .Where(x =>
                x.UserId == userId &&
                x.Purpose == purpose &&
                !x.Used &&
                x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (entity == null)
            return false;

        if (entity.Attempts >= _options.EmailConfirmationMaxAttempts)
            return false;

        entity.Attempts++;

        if (!string.Equals(entity.Code, code, StringComparison.Ordinal))
        {
            await _db.SaveChangesAsync(ct);
            return false;
        }

        entity.Used = true;
        await _db.SaveChangesAsync(ct);

        return true;
    }


    private static string GenerateNumericCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString($"D{length}");
    }
}
