
using Asas.Identity.Application.Contracts;
using Asas.Permission.Contracts;
using Asas.Permission.Domain;
using Asas.Tenancy.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Asas.Permission.Application
{
    public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionChecker _checker;
        private readonly ICurrentTenant _tenant;
        private readonly ICurrentUser _current; // your Identity abstraction

        public PermissionHandler(IPermissionChecker checker, ICurrentTenant tenant, ICurrentUser current)
        { _checker = checker; _tenant = tenant; _current = current; }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement req)
        {
            if (_current.Id is null) return;
            if(_tenant is null) return;
            if (await _checker.IsGrantedAsync(_current.Id.Value, req.Name, _tenant.Id))
                context.Succeed(req);
        }
    }
}
