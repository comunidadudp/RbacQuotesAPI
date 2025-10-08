using RbacApi.Data.Entities;

namespace RbacApi.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task AddUserAsync(User user);
    }
}
