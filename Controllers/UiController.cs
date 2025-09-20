using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.DTOs;
using RbacApi.Filters;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UiController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public UiController(IMenuService menuService)
        {
            _menuService = menuService;
        }


        [HttpGet("menu")]
        [Audit("ui.menu")]
        public async Task<ApiResponse<IEnumerable<MenuItemDTO>>> GetMenu()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var response = await _menuService.GetMenuForUserAsync(userId!);
            HttpContext.Response.StatusCode = response.Status;
            return response;
        }
    }
}
