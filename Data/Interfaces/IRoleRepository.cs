using RbacApi.Data.Entities;
using RbacApi.Enums;

namespace RbacApi.Data.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(RoleType id);
    }
}
