using Microsoft.AspNetCore.Authorization;
using RbacApi.Handlers.Models;

namespace RbacApi.Handlers;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;

        if (!user.Identity?.IsAuthenticated ?? false)
        {
            return Task.CompletedTask;
        }

        var userPermissions = user!.FindAll("permissions").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (userPermissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
