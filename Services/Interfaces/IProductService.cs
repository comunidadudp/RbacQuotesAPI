using RbacApi.DTOs;
using RbacApi.Responses;

namespace RbacApi.Services.Interfaces;

public interface IProductService
{
    Task<ApiResponseBase> CreateAsync(CreateProductRequest request);
    Task<ApiResponseBase> GetByIdAsync(string id);
    Task<ApiResponseBase> GetBySlugAsync(string slug);
    Task<ApiResponseBase> GetAllAsync();
}
