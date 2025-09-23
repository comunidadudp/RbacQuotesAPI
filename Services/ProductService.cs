using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.DTOs;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(CollectionsProvider collections)
    {
        _products = collections.Products;
    }

    public async Task<ApiResponseBase> CreateAsync(CreateProductRequest request)
    {
        var existing = await _products.Find(p => p.Code == request.Code).FirstOrDefaultAsync();

        if (existing != null)
            return ApiResponse.BadRequest("El producto ya existe");

        var product = new Product
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice
        };

        await _products.InsertOneAsync(product);

        return ApiResponse<string>.Ok(product.Id);
    }
}
