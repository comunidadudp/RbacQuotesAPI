using Mapster;
using MongoDB.Bson;
using RbacApi.Data.Entities;
using RbacApi.DTOs;

namespace RbacApi.Mapping;

public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        MapToProduct(config);
        MapToGetProductDTO(config);
        MapToGetProductDetailDTO(config);
    }

    private void MapToGetProductDetailDTO(TypeAdapterConfig config)
    {
        config.NewConfig<Product, GetProductDetailDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SKU, src => src.SKU)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug)
            .Map(dest => dest.Category, src => src.Category)
            .Map(dest => dest.Subcategory, src => src.Subcategory)
            .Map(dest => dest.ShortDescription, src => src.ShortDescription)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.BasePrice, src => src.BasePrice)
            .Map(dest => dest.Currency, src => src.Currency)
            .Map(dest => dest.ThumbnailUrl, src => src.ThumbnailKey ?? string.Empty)
            .Map(dest => dest.Images, src => src.ImageKeys == null ? null : src.ImageKeys.Select(img => new GetProductImageDTO(
                img.Key ?? string.Empty,
                img.Alt,
                img.Order,
                img.IsPrimary)).ToList())
            .Map(dest => dest.Features, src => src.Features == null ? null : src.Features.Select(f => new GetFeatureDTO(
                f.Key,
                f.Label,
                f.Value == null ? null! : BsonTypeMapper.MapToDotNetValue(f.Value),
                f.PriceImpact,
                f.HelpText)).ToList())
            .Map(dest => dest.Dimensions, src => src.Dimensions == null ? null : new GetDimensionsDTO(
                src.Dimensions.Length,
                src.Dimensions.Width,
                src.Dimensions.Height,
                src.Dimensions.Unit ?? "m"))
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(dest => dest.UsageApplicability, src => src.UsageApplicability)
            .Map(dest => dest.DefaultConfiguration, src => src.DefaultConfiguration == null ? null : src.DefaultConfiguration.Select(f => new GetFeatureDTO(
                f.Key,
                f.Label,
                f.Value == null ? null! : BsonTypeMapper.MapToDotNetValue(f.Value),
                f.PriceImpact,
                f.HelpText)).ToList())
            .Map(dest => dest.WarranyMonths, src => src.WarrantyMonths)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }

    private static void MapToGetProductDTO(TypeAdapterConfig config)
    {
        config.NewConfig<Product, GetProductDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SKU, src => src.SKU)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Slug, src => src.Slug)
            .Map(dest => dest.Category, src => src.Category)
            .Map(dest => dest.Subcategory, src => src.Subcategory)
            .Map(dest => dest.ShortDescription, src => src.ShortDescription)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.BasePrice, src => src.BasePrice)
            .Map(dest => dest.Currency, src => src.Currency)
            .Map(dest => dest.ThumbnailUrl, src => src.ThumbnailKey ?? string.Empty)
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }

    private static void MapToProduct(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProductRequest, Product>()
            .Map(dest => dest.SKU, src => src.SKU ?? string.Empty)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Slug, src => src.Slug ?? string.Empty)
            .Map(dest => dest.Category, src => src.Category ?? string.Empty)
            .Map(dest => dest.Subcategory, src => src.Subcategory ?? string.Empty)
            .Map(dest => dest.ShortDescription, src => src.ShortDescription ?? string.Empty)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.BasePrice, src => src.BasePrice ?? 0)
            .Map(dest => dest.ImageKeys, src => src.Images == null ? new List<ProductImage>() : src.Images.Select(img => new ProductImage
            {
                Alt = img.Alt,
                Order = img.Order ?? 0,
                IsPrimary = img.IsPrimary ?? false
            }).ToList())
            .Map(dest => dest.Features, src => src.Features == null ? new List<FeatureOption>() : src.Features.Select(f => new FeatureOption
            {
                Key = f.Key ?? string.Empty,
                Label = f.Label ?? string.Empty,
                Value = f.Value == null ? null : BsonValue.Create(f.Value),
                PriceImpact = f.PriceImpact ?? 0,
                HelpText = f.HelpText
            }).ToList())
            .Map(dest => dest.Dimensions, src => src.Dimensions == null ? null : new Dimensions
            {
                Length = src.Dimensions.Length ?? 0,
                Width = src.Dimensions.Width ?? 0,
                Height = src.Dimensions.Height ?? 0,
                Unit = src.Dimensions.Unit
            })
            .Map(dest => dest.Tags, src => src.Tags ?? new List<string>())
            .Map(dest => dest.UsageApplicability, src => src.UsageApplicability ?? new Dictionary<string, bool>())
            .Map(dest => dest.DefaultConfiguration, src => src.DefaultConfiguration == null ? new List<FeatureOption>() : src.DefaultConfiguration.Select(f => new FeatureOption
            {
                Key = f.Key ?? string.Empty,
                Label = f.Label ?? string.Empty,
                Value = f.Value == null ? null : BsonValue.Create(f.Value),
                PriceImpact = f.PriceImpact ?? 0,
                HelpText = f.HelpText
            }).ToList())
            .Map(dest => dest.WarrantyMonths, src => src.WarranyMonths)
            .Map(dest => dest.CreatedAt, _ => DateTime.UtcNow)
            .Map(dest => dest.IsActive, _ => true);
    }
}