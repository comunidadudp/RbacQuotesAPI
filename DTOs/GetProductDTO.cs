namespace RbacApi.DTOs;

public record GetProductDTO(
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
    List<string>? Tags,
    bool IsActive
)
{
    public string ThumbnailUrl { get; set; } = ThumbnailUrl;
}