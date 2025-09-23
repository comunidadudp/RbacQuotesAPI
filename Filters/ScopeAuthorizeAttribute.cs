using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;

namespace RbacApi.Filters;

public class ScopeAuthorizeAttribute(string[] scopes) : Attribute, IAsyncAuthorizationFilter, IAuthorizeData
{
    public string[] Scopes { get; set; } = scopes;
    public string? Policy { get; set; }
    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await ValidateScopesAsync(context, user);
    }

    private async Task ValidateScopesAsync(AuthorizationFilterContext context, ClaimsPrincipal user)
    {
        var collections = context.HttpContext.RequestServices.GetRequiredService<CollectionsProvider>();

        var allPermissions = await collections.Permissions.Find(FilterDefinition<Permission>.Empty).ToListAsync();

        var userPermissions = user!.FindAll("permissions").Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (userPermissions.Count == 0)
        {
            context.Result = new ForbidResult();
            context.HttpContext.Response.Headers.Append("WWW-Authenticate", "You haven't permissions");
            return;
        }

        if (!Scopes.Any(scope => userPermissions.Contains(scope)))
        {
            context.Result = new ForbidResult();
            context.HttpContext.Response.Headers.Append("WWW-Authenticate", $"Bearer error=\"insufficient_scopes\"");
            return;
        }
    }
}
