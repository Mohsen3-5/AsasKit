namespace Asas.Tenancy.Infrastructure.EF;

public sealed class TenancyModelOptions
{
    public string TenantIdPropertyName { get; set; } = "TenantId";
    public int TenantIdMaxLength { get; set; } = 64;

    // Convention control
    public bool ScopeAllByDefault { get; set; } = true;
    public List<string> IncludeNamespaces { get; } = new(); // e.g. "Asas."
    public List<string> ExcludeNamespaces { get; } = new() { "Asas.Tenancy" }; // don’t scope the Tenancy module itself
    public HashSet<Type> IncludeTypes { get; } = new();
    public HashSet<Type> ExcludeTypes { get; } = new();

    public Func<Type, bool>? IsTenantScoped { get; set; } // ultimate override
}
