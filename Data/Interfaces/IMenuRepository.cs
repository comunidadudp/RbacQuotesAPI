using RbacApi.Data.Entities;

namespace RbacApi.Data.Interfaces
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuItem>> GetAllAsync(ISpecification<MenuItem> specification);
    }
}
