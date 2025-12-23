namespace Asas.Identity.Infrastructure;

public class GoogleOptions { public string? ClientId { get; set; } }
public class FacebookOptions { public string? AppId { get; set; } }
public class AppleOptions { public string? ClientId { get; set; } }

public class ExternalAuthOptions
{
    public GoogleOptions Google { get; set; } = new();
    public FacebookOptions Facebook { get; set; } = new();
    public AppleOptions Apple { get; set; } = new();
}

public class AsasIdentityOptions
{
    public bool RequireConfirmedEmail { get; set; } = true;

    public int EmailConfirmationCodeLength { get; set; } = 6;
    public int EmailConfirmationCodeTtlMinutes { get; set; } = 10;
    public int EmailConfirmationMaxAttempts { get; set; } = 5;

    public ExternalAuthOptions ExternalAuth { get; set; } = new();
}
