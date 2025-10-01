using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class Product
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("sku")]
    public string SKU { get; set; } = default!;

    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("slug")]
    public string Slug { get; set; } = default!;

    [BsonElement("category")]
    public string Category { get; set; } = default!;

    [BsonElement("subcategory")]
    public string Subcategory { get; set; } = default!;

    [BsonElement("shortDescription")]
    public string ShortDescription { get; set; } = default!;

    [BsonElement("Description")]
    public string? Description { get; set; } = default!;

    [BsonElement("basePrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BasePrice { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "COP";

    [BsonElement("images")]
    public List<ProductImage> ImageKeys { get; set; } = [];

    [BsonElement("thumbnail")]
    public string ThumbnailKey { get; set; } = default!;

    [BsonElement("features")]
    public List<FeatureOption> Features { get; set; } = [];

    [BsonElement("dimensions")]
    [BsonIgnoreIfNull]
    public Dimensions? Dimensions { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    [BsonElement("usageApplicability")]
    public Dictionary<string, bool> UsageApplicability { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updateAt")]
    public DateTime? UpdateAt { get; set; }

    [BsonElement("defaultConfiguration")]
    [BsonIgnoreIfNull]
    public List<FeatureOption> DefaultConfiguration { get; set; } = [];

    [BsonElement("warranyMonths")]
    public int? WarrantyMonths { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; }
}

public class ProductImage
{
    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("alt")]
    public string? Alt { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("isPrimary")]
    public bool IsPrimary { get; set; }
}

public class FeatureOption
{
    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("label")]
    public string Label { get; set; } = string.Empty;

    [BsonElement("value")]
    public BsonValue? Value { get; set; } = string.Empty;

    [BsonElement("priceImpact")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PriceImpact { get; set; }

    [BsonElement("helpText")]
    public string? HelpText { get; set; }
}

public class Dimensions
{
    [BsonElement("length")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Length { get; set; }

    [BsonElement("width")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Width { get; set; }

    [BsonElement("height")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Height { get; set; }

    [BsonElement("unit")]
    public string? Unit { get; set; }
}
