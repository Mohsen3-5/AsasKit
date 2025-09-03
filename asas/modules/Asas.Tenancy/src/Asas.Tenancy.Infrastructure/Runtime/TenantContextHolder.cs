
namespace Asas.Tenancy.Infrastructure.Runtime;
internal static class TenantContextHolder
{
    private static readonly AsyncLocal<TenantContext?> _current = new();
    public static TenantContext? Current { get => _current.Value; set => _current.Value = value; }
}
