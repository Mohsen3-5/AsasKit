using Asas.Core.EF;

namespace Asas.Tenancy.Domain
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Identifier { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public required string Host { get; set; }
    }
}
