
using Microsoft.AspNetCore.Authorization;

namespace Asas.Permission.Domain
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public string Name { get; }
        public PermissionRequirement(string name) => Name = name;
    }
}
