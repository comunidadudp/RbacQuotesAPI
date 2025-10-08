using MongoDB.Driver;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;

namespace RbacApi.Data.Repositories
{
    public class PermissionRepository(CollectionsProvider collections) : IPermissionRepository
    {
        private readonly IMongoCollection<Permission> _permissions = collections.Permissions;

        public async Task<IReadOnlyList<Permission>> GetAllAsync()
            => await _permissions.Find(FilterDefinition<Permission>.Empty).ToListAsync();

        public async Task<Permission?> GetByIdAsync(string id)
            => await _permissions.Find(p => p.Code == id).FirstOrDefaultAsync();
    }
}
