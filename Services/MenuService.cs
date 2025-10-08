using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using RbacApi.DTOs;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;
using RbacApi.Specs;

namespace RbacApi.Services;

public class MenuService : IMenuService
{
    private readonly IUserService _userService;
    private readonly IMenuRepository _menuRepository;

    public MenuService(IUserService userService, IMenuRepository menuRepository)
    {
        _userService = userService;
        _menuRepository = menuRepository;
    }

    public async Task<ApiResponse<IEnumerable<MenuItemDTO>>> GetMenuForUserAsync(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<IEnumerable<MenuItemDTO>>.Ok([]);

        var effective = (await _userService.GetEffectivePermissionsForUserAsync(user)).ToHashSet();

        var spec = new MenuItemSpec();
        spec.AddOrderBy(m => m.Order);
        var roots = await _menuRepository.GetAllAsync(spec);

        var visible = new List<MenuItemDTO>();

        foreach (var root in roots)
        {
            if (TryFilterMenu(root, effective, out var dto))
                visible.Add(dto);
        }

        return ApiResponse<IEnumerable<MenuItemDTO>>.Ok(visible.OrderBy(m => m.Order));
    }

    private static bool TryFilterMenu(MenuItem item, HashSet<string> perms, out MenuItemDTO dto)
    {
        dto = new MenuItemDTO
        {
            Id = item.Id,
            Label = item.Label,
            Icon = item.Icon,
            Route = item.Route,
            Order = item.Order,
        };

        var required = item.RequiredPermissions ?? [];
        bool allowed = !required.Any() || required.All(r => perms.Contains(r));

        // children
        foreach (var child in item.Children)
        {
            if (TryFilterMenu(child, perms, out var childDTO))
                dto.Children.Add(childDTO);
        }

        var include = allowed || dto.Children.Count > 0;
        return include;
    }
}
