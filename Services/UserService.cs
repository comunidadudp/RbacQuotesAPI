using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class UserService(
    IUserRepository userRepository, 
    IRoleRepository roleRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<User?> GetByIdAsync(string id)
        => await _userRepository.GetByIdAsync(id);

    public async Task<IEnumerable<string>> GetEffectivePermissionsForUserAsync(User user)
    {
        var permissions = new HashSet<string>(user.Permissions ?? []);

        // 1. Permisos del rol
        var role = await _roleRepository.GetByIdAsync(user.RoleId);
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
