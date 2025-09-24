namespace RbacApi.DTOs;

public record CreateProductRequest(
   string? Code,
   string? Name,
   string? Description,
   decimal? BasePrice,
   IFormFile? Image
);
