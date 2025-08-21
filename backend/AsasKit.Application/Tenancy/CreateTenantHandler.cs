using AsasKit.Application.Abstractions;
using AsasKit.Application.Abstractions.Persistence;
using AsasKit.Domain.Tenancy;
using FluentValidation;
using MediatR;

namespace AsasKit.Application.Tenancy;

public sealed class CreateTenantHandler(
    ITenantRepository tenants,
    IMembershipRepository memberships,
    ICurrentUser currentUser,
    IUnitOfWork uow
) : IRequestHandler<CreateTenant, Guid>
{
    public async Task<Guid> Handle(CreateTenant req, CancellationToken ct)
    {
        // Basic guard; detailed regex checks are in the validator you already added
        if (await tenants.SlugExistsAsync(req.Slug, ct))
            throw new ValidationException($"Slug '{req.Slug}' is already taken.");

        var tenant = new Tenant(req.Name, req.Slug);
        await tenants.AddAsync(tenant, ct);

        // If we have an authenticated user, make them Owner of the new tenant
        if (currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty)
        {
            var owner = new Membership(tenant.Id, currentUser.UserId, role: "Owner");
            await memberships.AddAsync(owner, ct);
        }

        await uow.SaveChangesAsync(ct);
        return tenant.Id;
    }
}