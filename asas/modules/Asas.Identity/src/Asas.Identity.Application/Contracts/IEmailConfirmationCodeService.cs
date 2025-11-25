using Asas.Identity.Domain.Entities;

namespace Asas.Identity.Application.Contracts;
public interface IEmailConfirmationCodeService
{
    Task<string> GenerateAndStoreAsync(AsasUser user, bool forPasswordReset = false, CancellationToken ct = default);
    Task<bool> VerifyAsync(Guid userId, string code, bool forPasswordReset = false, CancellationToken ct = default);
}
