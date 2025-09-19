using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.DTOs;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class MenuService : IMenuService
{
    private readonly CollectionsProvider _collections;
    private readonly IUserService _userService;

    public MenuService(CollectionsProvider collections, IUserService userService)
    {
        _collections = collections;
        _userService = userService;
    }

    public async Task<ApiResponse<IEnumerable<MenuItemDTO>>> GetMenuForUserAsync(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<IEnumerable<MenuItemDTO>>.Ok([]);

        var effective = (await _userService.GetEffectivePermissionsForUserAsync(user)).ToHashSet();

        var roots = await _collections.MenuItems.Find(FilterDefinition<MenuItem>.Empty).SortBy(m => m.Order).ToListAsync();

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
