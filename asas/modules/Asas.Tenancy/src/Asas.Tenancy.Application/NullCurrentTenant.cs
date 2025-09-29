
using Asas.Tenancy.Contracts;

namespace Asas.Tenancy.Application;
public sealed class NullCurrentTenant : ICurrentTenant
{
    public bool IsSet => false;

    int? ICurrentTenant.Id => null;
}
