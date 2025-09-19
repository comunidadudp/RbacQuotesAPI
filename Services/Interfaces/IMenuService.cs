using RbacApi.DTOs;
using RbacApi.Responses;

namespace RbacApi.Services.Interfaces;

public interface IMenuService
{
    Task<ApiResponse<IEnumerable<MenuItemDTO>>> GetMenuForUserAsync(string userId);
}
