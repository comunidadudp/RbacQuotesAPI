namespace RbacApi.DTOs;

public record GetProductDetailDTO(
   string Id,
   string SKU,
   string Name,
   string Slug,
   string Category,
   string Subcategory,
   string ShortDescription,
   string? Description,
   decimal BasePrice,
   string Currency,
   string ThumbnailUrl,
   List<GetProductImageDTO>? Images,
   List<GetFeatureDTO>? Features,
   GetDimensionsDTO? Dimensions,
   List<string>? Tags,
   Dictionary<string, bool>? UsageApplicability,
   List<GetFeatureDTO>? DefaultConfiguration,
   int? WarranyMonths,
   bool IsActive
)
{
   public string ThumbnailUrl { get; set; } = ThumbnailUrl;
}


public record GetProductImageDTO(
   string ImageUrl,
   string? Alt,
   int Order,
   bool IsPrimary
)
{
   public string ImageUrl { get; set; } = ImageUrl;
}

public record GetFeatureDTO(
   string Key,
   string Label,
   object Value,
   decimal PriceImpact,
   string? HelpText
);


public record GetDimensionsDTO(
   decimal Length,
   decimal Width,
   decimal Height,
   string Unit
);