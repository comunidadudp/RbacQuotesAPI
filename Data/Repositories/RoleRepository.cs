using MongoDB.Driver;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using RbacApi.Enums;

namespace RbacApi.Data.Repositories
{
    public class RoleRepository(CollectionsProvider collections) : IRoleRepository
    {
        private readonly IMongoCollection<Role> _roles = collections.Roles;

        public async Task<Role?> GetByIdAsync(RoleType id)
            => await _roles.Find(r => r.Id == id).FirstOrDefaultAsync();
    }
}
