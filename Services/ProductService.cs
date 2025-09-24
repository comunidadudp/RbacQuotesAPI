using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.DTOs;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Storage.Models;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly IStorageService _storageService;

    public ProductService(CollectionsProvider collections, IStorageService storageService)
    {
        _products = collections.Products;
        _storageService = storageService;
    }

    public async Task<ApiResponseBase> CreateAsync(CreateProductRequest request)
    {
        var existing = await _products.Find(p => p.Code == request.Code).FirstOrDefaultAsync();

        if (existing != null)
            return ApiResponse.BadRequest("El producto ya existe");

        string extension = Path.GetExtension(request.Image.FileName);
        string storageKey = $"products/{request.Code}/poster{extension}";

        var storageObject = new PutStorageObject(
            storageKey,
            request.Image.OpenReadStream(),
            request.Image.ContentType
        );

        await _storageService.CreateFileInStorageAsync(storageObject);

        var product = new Product
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice.HasValue ? request.BasePrice.Value : 0,
            ImageKey = storageKey
        };

        await _products.InsertOneAsync(product);

        return ApiResponse<string>.Ok(product.Id);
    }

    public async Task<ApiResponseBase> GetByIdAsync(string id)
    {
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (product == null)
            return ApiResponse.NotFound($"No se encontró el producto con id '{id}'");

        string imageUrl = await _storageService.GenerateFileUrlAsync(product.ImageKey);

        var productDTO = new GetProductDTO(
            product.Id,
            product.Code,
            product.Name,
            product.Description,
            product.BasePrice,
            imageUrl
        );

        return ApiResponse<GetProductDTO>.Ok(productDTO);
    }
}
