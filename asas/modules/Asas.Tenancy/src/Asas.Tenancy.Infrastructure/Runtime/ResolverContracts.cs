namespace Asas.Tenancy.Infrastructure.Runtime;

public sealed class ResolveResult
{
    public string? Id { get; init; }
    public string? Slug { get; init; }
    public bool HasValue => !string.IsNullOrWhiteSpace(Id) || !string.IsNullOrWhiteSpace(Slug);
}

public interface ITenantResolver { Task<ResolveResult> ResolveAsync(CancellationToken ct = default); }
