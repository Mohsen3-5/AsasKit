// Tenancy/CreateTenant.cs
using MediatR;

namespace AsasKit.Application.Tenancy;
public sealed record CreateTenant(string Name, string Slug) : IRequest<Guid>;
