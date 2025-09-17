using RbacApi.DTOs;

namespace RbacApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterRequest request);
    Task<AuthResponseDTO> LoginAsync(LoginRequest request);
}