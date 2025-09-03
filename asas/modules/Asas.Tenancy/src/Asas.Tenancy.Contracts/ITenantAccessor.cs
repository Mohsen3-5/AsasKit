using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asas.Tenancy.Contracts;
public interface ITenantAccessor
{
    TenantInfo? Current { get; }
    bool TryGet(out TenantInfo? tenant);
}