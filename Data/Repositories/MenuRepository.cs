using MongoDB.Driver;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;

namespace RbacApi.Data.Repositories
{
    public class MenuRepository(CollectionsProvider collections) : IMenuRepository
    {
        public async Task<IEnumerable<MenuItem>> GetAllAsync(ISpecification<MenuItem> specification)
         => await BaseRepository<MenuItem>.GetAllBySpecAsync(collections.MenuItems.AsQueryable(), specification);
    }
}
