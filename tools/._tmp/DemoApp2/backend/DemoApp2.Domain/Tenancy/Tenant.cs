// Tenancy/Tenant.cs

using DemoApp2.Domain.Common.Entity;

namespace DemoApp2.Domain.Tenancy;
using DemoApp2.Domain.Common;

public sealed class Tenant : Entity<Guid>
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public TenantStatus Status { get; private set; } = TenantStatus.Active;

    private Tenant() { Name = Slug = ""; } // EF
    public Tenant(string name, string slug)
    {
        Id = Guid.NewGuid();
        Name = name;
        Slug = slug;
    }
}

public enum TenantStatus { Active = 1, Suspended = 2 }