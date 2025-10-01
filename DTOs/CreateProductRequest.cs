namespace RbacApi.DTOs;

public record CreateProductRequest(
   string? SKU,
   string? Name,
   string? Slug,
   string? Category,
   string? Subcategory,
   string? ShortDescription,
   string? Description,
   decimal? BasePrice,
   List<ProductImageDTO>? Images,
   List<FeatureDTO>? Features,
   DimensionsDTO? Dimensions,
   List<string>? Tags,
   Dictionary<string, bool>? UsageApplicability,
   List<FeatureDTO>? DefaultConfiguration,
   int? WarranyMonths,
   IFormFile? Image
);

public record ProductImageDTO(
   IFormFile? File,
   string? Alt,
   int? Order,
   bool? IsPrimary
);

public record FeatureDTO(
   string? Key,
   string? Label,
   object? Value,
   decimal? PriceImpact,
   string? HelpText
);


public record DimensionsDTO(
   decimal? Length,
   decimal? Width,
   decimal? Height,
   string? Unit
);
