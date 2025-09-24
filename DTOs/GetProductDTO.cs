namespace RbacApi.DTOs;

public record GetProductDTO(
    string Id,
    string Code,
    string Name,
    string Description,
    decimal BasePrice,
    string ImageUrl
);