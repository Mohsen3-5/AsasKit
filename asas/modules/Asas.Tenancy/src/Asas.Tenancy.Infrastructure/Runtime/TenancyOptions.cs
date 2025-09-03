namespace Asas.Tenancy.Infrastructure.Runtime;
public sealed class TenancyOptions
{
    // resolution order: "route","header","claims","subdomain"
    public IList<string> ResolutionOrder { get; } = new List<string> { "route", "header", "claims", "subdomain" };
    public string HeaderName { get; set; } = "X-Tenant";
    public string RouteParamName { get; set; } = "tenant";
    public string ClaimType { get; set; } = "tenant_id";
    public string? RootDomain { get; set; } // e.g. "example.com"
}