using Mapster;
using Microsoft.Extensions.Options;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using RbacApi.DTOs;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Storage.Models;
using RbacApi.Infrastructure.Validators;
using RbacApi.Pagination;
using RbacApi.QueryFilters;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;
using RbacApi.Specs;

namespace RbacApi.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IStorageService _storageService;
    private readonly CreateProductRequestValidator _createValidator;
    private readonly ISigner _signer;
    private readonly PaginationOptions _pagination;

    public ProductService(
        IProductRepository productRepository,
        IStorageService storageService,
        CreateProductRequestValidator createValidator,
        ISigner signer,
        IOptions<PaginationOptions> options)
    {
        _productRepository = productRepository;
        _storageService = storageService;
        _createValidator = createValidator;
        _signer = signer;
        _pagination = options.Value;
    }

    public async Task<ApiResponseBase> CreateAsync(CreateProductRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name!) : Slugify(request.Slug);

        if (await _productRepository.GetBySlugAsync(slug) != null)
            return ApiResponse.BadRequest("El slug ya existe. Por favor, elige otro.");

        var existing = await _productRepository.GetBySKUAsync(request.SKU!);
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

        await _productRepository.AddAsync(product);
        return ApiResponse<string>.Ok(product.Id);
    }

    public async Task<ApiResponseBase> GetAllAsync(ProductQueryFilter filter)
    {
        var spec = new ProductSpec();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            spec.AndAlso(p => p.Name.ToLower().Contains(filter.Name.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Category))
            spec.AndAlso(p => p.Category.ToLower() == filter.Category.ToLower());

        if (!string.IsNullOrWhiteSpace(filter.Subcategory))
            spec.AndAlso(p => p.Subcategory.ToLower() == filter.Subcategory.ToLower());

        int count = await _productRepository.CountAsync(spec);

        // filtrado de paginación
        filter.PageIndex ??= _pagination.DefaultPageIndex;
        filter.PageSize ??= _pagination.DefaultPageSize;
        filter.PageSize = Math.Min(filter.PageSize.Value, _pagination.MaxPageSize);

        spec.ApplyPaging(filter.PageSize.Value * (filter.PageIndex.Value - 1), filter.PageSize.Value);

        switch (filter.Sort)
        {
            case SortOrder.Ascending:
                spec.AddOrderBy(p => p.CreatedAt);
                break;
            case SortOrder.Descending:
                spec.AddOrderByDescending(p => p.CreatedAt);
                break;
            default:
                spec.AddOrderByDescending(p => p.CreatedAt);
                break;
        }

        IEnumerable<Product> products = await _productRepository.GetAllAsync(spec);
        var dtos = products.Adapt<List<GetProductDTO>>();
        foreach (var img in dtos)
        {
            img.ThumbnailUrl = _signer.GetSignedUrl(img.ThumbnailUrl);
        }

        var result = PaginatedResult<GetProductDTO>.Create(dtos, filter, count);

        return ApiResponse<PaginatedResult<GetProductDTO>>.Ok(result);
    }

    public async Task<ApiResponseBase> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);

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
        var product = await _productRepository.GetBySlugAsync(slug);

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
