namespace Asas.Identity.Infrastructure;
public class AsasIdentityOptions
{
    public bool RequireConfirmedEmail { get; set; } = true;

    public int EmailConfirmationCodeLength { get; set; } = 6;
    public int EmailConfirmationCodeTtlMinutes { get; set; } = 10;
    public int EmailConfirmationMaxAttempts { get; set; } = 5;
}
