using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asas.Tenancy.Contracts;

namespace Asas.Tenancy.Infrastructure.Runtime;
internal sealed class TenantContext
{
    public TenantInfo? Tenant { get; }
    public TenantContext(TenantInfo? tenant) => Tenant = tenant;
}