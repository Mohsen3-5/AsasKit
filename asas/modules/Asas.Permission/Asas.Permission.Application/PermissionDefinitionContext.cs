
using Asas.Permission.Domain.Entity;
namespace Asas.Permission.Application
{
    public sealed class PermissionDefinitionContext
    {
        private readonly List<AsasPermission> _items = new();
        public AsasPermission Add(string name, string display, string? desc = null, string group = "General")
        {
            var p = new AsasPermission { Name = name, DisplayName = display, Description = desc, Group = group };
            _items.Add(p);
            return p;
        }
        public IReadOnlyList<AsasPermission> Items => _items;
    }
}
