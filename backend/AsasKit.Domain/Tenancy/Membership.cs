// Tenancy/Membership.cs

using AsasKit.Domain.Common.Entity;

namespace AsasKit.Domain.Tenancy;
using AsasKit.Domain.Common;

public sealed class Membership : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = "Member";

    private Membership() { }
    public Membership(Guid userId, string role = "Member")
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Role = role;
    }
}