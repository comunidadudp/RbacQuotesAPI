using MongoDB.Driver;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;

namespace RbacApi.Data.Repositories
{
    public class UserRepository(CollectionsProvider collections) : IUserRepository
    {
        public async Task AddUserAsync(User user)
            => await collections.Users.InsertOneAsync(user);

        public async Task<User?> GetByEmailAsync(string email)
            => await collections.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<User?> GetByIdAsync(string id)
            => await collections.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }
}
