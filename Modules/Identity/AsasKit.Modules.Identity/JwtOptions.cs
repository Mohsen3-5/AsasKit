namespace AsasKit.Modules.Identity;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "AsasKit";
    public string Audience { get; set; } = "AsasKit.Client";
    public string Key { get; set; } = ""; // 32+ chars
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}
