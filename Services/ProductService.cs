using Mapster;
using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.DTOs;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Storage.Models;
using RbacApi.Infrastructure.Validators;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly IStorageService _storageService;
    private readonly CreateProductRequestValidator _createValidator;
    private readonly ISigner _signer;

    public ProductService(
        CollectionsProvider collections,
        IStorageService storageService,
        CreateProductRequestValidator createValidator,
        ISigner signer)
    {
        _products = collections.Products;
        _storageService = storageService;
        _createValidator = createValidator;
        _signer = signer;
    }

    public async Task<ApiResponseBase> CreateAsync(CreateProductRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name!) : Slugify(request.Slug);

        var slugFilter = Builders<Product>.Filter.Eq(p => p.Slug, slug);

        if (await _products.Find(slugFilter).AnyAsync())
            return ApiResponse.BadRequest("El slug ya existe. Por favor, elige otro.");

        var existing = await _products.Find(p => p.SKU == request.SKU).FirstOrDefaultAsync();
        if (existing != null)
            return ApiResponse.BadRequest("El producto ya existe");

        var product = request.Adapt<Product>();
        product.Slug = slug;

        string extension = Path.GetExtension(request.Image!.FileName);
        string storageKey = $"products/{product.Slug}/thumbnail{extension}";

        var storageObject = new PutStorageObject(
            storageKey,
            request.Image.OpenReadStream(),
            request.Image.ContentType
        );

        product.ThumbnailKey = storageKey;

        await _storageService.CreateFileInStorageAsync(storageObject);


        foreach (var img in product.ImageKeys!)
        {
            var productImage = request.Images!.First(i => i.Order == img.Order).File!;

            string imgExtension = Path.GetExtension(productImage.FileName);
            string key = $"products/{product.Slug}/gallery-{img.Order}{imgExtension}";

            img.Key = key;
            img.Alt ??= $"{img.Order}-catalog";
            img.IsPrimary = img.Order == 1;

            var imgToUpload = new PutStorageObject(
                key,
                productImage.OpenReadStream(),
                productImage.ContentType
            );

            await _storageService.CreateFileInStorageAsync(imgToUpload);
        }

        await _products.InsertOneAsync(product);
        return ApiResponse<string>.Ok(product.Id);
    }

    public async Task<ApiResponseBase> GetAllAsync()
    {
        var products = await _products.Find(_ => true).ToListAsync();
        var dtos = products.Adapt<List<GetProductDTO>>();
        foreach (var img in dtos)
        {
            img.ThumbnailUrl = _signer.GetSignedUrl(img.ThumbnailUrl);
        }
        return ApiResponse<List<GetProductDTO>>.Ok(dtos);
    }

    public async Task<ApiResponseBase> GetByIdAsync(string id)
    {
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (product == null)
            return ApiResponse.NotFound($"No se encontró el producto con id '{id}'");

        string imageUrl = _signer.GetSignedUrl(product.ThumbnailKey);

        var productDTO = product.Adapt<GetProductDetailDTO>();
        productDTO.ThumbnailUrl = imageUrl;

        foreach (var img in productDTO.Images ?? [])
        {
            img.ImageUrl = _signer.GetSignedUrl(img.ImageUrl);
        }

        return ApiResponse<GetProductDetailDTO>.Ok(productDTO);
    }

    public async Task<ApiResponseBase> GetBySlugAsync(string slug)
    {
        var product = await _products.Find(p => p.Slug == Slugify(slug)).FirstOrDefaultAsync();

        if (product == null)
            return ApiResponse.NotFound($"No se encontró el producto con slug '{slug}'");

        string imageUrl = _signer.GetSignedUrl(product.ThumbnailKey);

        var productDTO = product.Adapt<GetProductDetailDTO>();
        productDTO.ThumbnailUrl = imageUrl;

        foreach (var img in productDTO.Images ?? [])
        {
            img.ImageUrl = _signer.GetSignedUrl(img.ImageUrl);
        }

        return ApiResponse<GetProductDetailDTO>.Ok(productDTO);
    }


    #region Privates

    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return Guid.NewGuid().ToString("n");
        var s = input.Trim().ToLowerInvariant();
        // Remplazos simples: sustituir espacios y caracteres no alfanum por '-'
        var sb = new System.Text.StringBuilder();
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-') sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '_' || ch == '.') sb.Append('-');
            // ignorar otros caracteres
        }
        var result = sb.ToString().Trim('-');
        return string.IsNullOrEmpty(result) ? Guid.NewGuid().ToString("n") : result;
    }

    #endregion
}
