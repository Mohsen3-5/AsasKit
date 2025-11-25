using Asas.Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Asas.Identity.Application.Contracts;

public sealed class NullEmailConfirmationCodeSender : IEmailConfirmationCodeSender
{
    private readonly ILogger<NullEmailConfirmationCodeSender> _logger;

    public NullEmailConfirmationCodeSender(ILogger<NullEmailConfirmationCodeSender> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationCodeAsync(AsasUser user, string code, bool isForgetPasswordConfirmation = false, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailConfirmationCodeSender invoked. No email was sent. " +
            "UserId={UserId}, Email={Email}, CodeLength={Length}. " +
            "NOTE: Host app should register a real IEmailConfirmationCodeSender.",
            user.Id,
            user.Email,
            code.Length
        );

        return Task.CompletedTask;
    }
}
