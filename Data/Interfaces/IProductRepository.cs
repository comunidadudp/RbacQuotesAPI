using RbacApi.Data.Entities;

namespace RbacApi.Data.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(ISpecification<Product> specification);
        Task<int> CountAsync(ISpecification<Product> specification);
        Task<Product?> GetBySlugAsync(string slug);
        Task<Product?> GetBySKUAsync(string sku);
        Task<Product?> GetByIdAsync(string id);
        Task AddAsync(Product product);
    }
}
