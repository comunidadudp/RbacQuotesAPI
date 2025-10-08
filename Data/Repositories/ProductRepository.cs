using MongoDB.Driver;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;

namespace RbacApi.Data.Repositories
{
    public sealed class ProductRepository(CollectionsProvider collections) : IProductRepository
    {
        private readonly IMongoCollection<Product> _products = collections.Products;

        public async Task AddAsync(Product product)
            => await _products.InsertOneAsync(product);

        public async Task<int> CountAsync(ISpecification<Product> specification)
            => await BaseRepository<Product>.CounAsync(_products.AsQueryable(), specification);

        public async Task<Product?> GetBySlugAsync(string slug)
            => await _products.Find(p => p.Slug == slug).FirstOrDefaultAsync();

        public async Task<IEnumerable<Product>> GetAllAsync(ISpecification<Product> specification)
            => await BaseRepository<Product>.GetAllBySpecAsync(_products.AsQueryable(), specification);

        public async Task<Product?> GetBySKUAsync(string sku)
            => await _products.Find(p => p.SKU == sku).FirstOrDefaultAsync();

        public async Task<Product?> GetByIdAsync(string id)
            => await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }
}
