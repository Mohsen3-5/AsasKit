// Tenancy/Membership.cs

using AsasKit.Domain.Common.Entity;

namespace AsasKit.Domain.Tenancy;
using AsasKit.Domain.Common;

public sealed class Membership : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = "Member";

    private Membership() { }
    public Membership(Guid tenantId, Guid userId, string role = "Member")
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        Role = role;
    }
}