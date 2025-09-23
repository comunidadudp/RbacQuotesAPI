using RbacApi.Data.Entities;

namespace RbacApi.Services.Interfaces;

public interface IPermissionService
{
    Task<Permission?> GetByIdAsync(string id);
    Task<IReadOnlyList<Permission>> GetAllAsync();
}
