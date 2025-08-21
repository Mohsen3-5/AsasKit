// Tenancy/CreateTenant.cs
using MediatR;

namespace AsasKit.Application.Tenancy;
public record CreateTenant(string Name, string Slug) : IRequest<Guid>;