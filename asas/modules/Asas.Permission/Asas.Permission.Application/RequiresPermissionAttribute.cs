
using Microsoft.AspNetCore.Authorization;

namespace Asas.Permission.Application;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequiresPermissionAttribute : AuthorizeAttribute
{
    public RequiresPermissionAttribute(string permission)
    {
        Policy = $"perm:{permission}";
    }
}