// Tenancy/CreateTenantValidator.cs
using FluentValidation;

namespace AsasKit.Application.Tenancy;
public sealed class CreateTenantValidator : AbstractValidator<CreateTenant>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
    }
}