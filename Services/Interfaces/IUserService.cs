using RbacApi.Data.Entities;

namespace RbacApi.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<string>> GetEffectivePermissionsForUserAsync(User user);
    Task<User?> GetByIdAsync(string id);
}
