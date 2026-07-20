using Microsoft.AspNetCore.Authorization;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.SharedKernel.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissionClaim = context.User.FindFirst(
            c => c.Type == "permission" && c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (permissionClaim is not null)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
