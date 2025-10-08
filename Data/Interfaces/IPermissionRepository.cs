using RbacApi.Data.Entities;

namespace RbacApi.Data.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IReadOnlyList<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(string id);
    }
}
