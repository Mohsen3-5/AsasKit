using Asas.Identity.Domain.Entities;

namespace Asas.Identity.Application.Contracts;
public interface IEmailConfirmationCodeSender
{
    Task SendConfirmationCodeAsync(AsasUser user, string code, bool isForgetPasswordConfirmation = false, CancellationToken ct = default);
}
