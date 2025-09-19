using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class UserService(CollectionsProvider collections) : IUserService
{
    private readonly CollectionsProvider _collections = collections;

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _collections.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<string>> GetEffectivePermissionsForUserAsync(User user)
    {
        var permissions = new HashSet<string>(user.Permissions ?? []);

        // 1. Permisos del rol
        var role = await _collections.Roles.Find(r => r.Id == user.RoleId).FirstOrDefaultAsync();
        var rolePerms = role?.Permissions ?? [];

        // 2. Permisos propios
        var userGrants = user.Permissions ?? [];

        // 3.Permisos revocados
        var userRevokes = user.RevokedPermissions ?? [];

        // 4. Permisos resultantes
        var set = new HashSet<string>(rolePerms);

        foreach (var p in userGrants) set.Add(p);
        foreach (var r in userRevokes) set.Remove(r);

        return set;
    }
}
