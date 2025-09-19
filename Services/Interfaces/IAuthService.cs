using RbacApi.DTOs;
using RbacApi.Responses;

namespace RbacApi.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponseBase> RegisterAsync(RegisterRequest request);
    Task<ApiResponseBase> LoginAsync(LoginRequest request);
    Task<ApiResponseBase> RefreshAsync(RefreshRequest request);
    Task<ApiResponseBase> RevokeRefreshTokenAsync(RefreshRequest request);
}