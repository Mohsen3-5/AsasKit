using Asas.Permission.Contracts;
using Asas.Permission.Infrastructure;

namespace Asas.Permission.Application
{
    /// <summary>
    /// Provides meta-permissions for managing the permission system itself
    /// </summary>
    public class SystemPermissionDefinitionProvider : IPermissionDefinitionProvider
    {
        public void Define(PermissionDefinitionContext ctx)
        {
            // Super Admin Permission - Grants full access to everything
            ctx.Add(
                "Admin",
                "System Administrator",
                "Super admin permission - grants full access to all features and bypasses all permission checks",
                "Super Admin");

            // Permission Management Group
            ctx.Add(
                "Permissions.View",
                "View Permissions",
                "Can view all available permissions in the system",
                "Permission Management");

            ctx.Add(
                "Permissions.Create",
                "Create Permissions",
                "Can create new permission definitions (reserved for future use)",
                "Permission Management");

            ctx.Add(
                "Permissions.Edit",
                "Edit Permissions",
                "Can edit existing permission definitions (reserved for future use)",
                "Permission Management");

            ctx.Add(
                "Permissions.Delete",
                "Delete Permissions",
                "Can delete permission definitions (reserved for future use)",
                "Permission Management");

            ctx.Add(
                "Permissions.Grant",
                "Grant Permissions",
                "Can grant permissions to roles or users",
                "Permission Management");

            ctx.Add(
                "Permissions.Revoke",
                "Revoke Permissions",
                "Can revoke permissions from roles or users",
                "Permission Management");

            ctx.Add(
                "Roles.ManagePermissions",
                "Manage Role Permissions",
                "Can view and manage permissions assigned to roles",
                "Permission Management");

            ctx.Add(
                "Users.ManagePermissions",
                "Manage User Permissions",
                "Can view and manage user-specific permission overrides",
                "Permission Management");
        }
    }
}
