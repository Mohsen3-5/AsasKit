using AsasKit.Modules.Identity.Entities;
using Kernel;

namespace AsasKit.Modules.Identity;

[DependsOn(typeof(IdentityStartupModule))]
public sealed class IdentityUowModule
    : AsasKit.ModulesUowStartupModule<AsasIdentityDbContext<AsasUser>>
{ }
