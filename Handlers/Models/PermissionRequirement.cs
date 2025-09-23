using Microsoft.AspNetCore.Authorization;

namespace RbacApi.Handlers.Models;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
