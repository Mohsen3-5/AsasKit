using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asas.Tenancy.Contracts;
public sealed class TenantInfo
{
    public Guid Id { get; init; }   // the value we filter with
    public string? Slug { get; init; }
    public string? Name { get; init; }
}