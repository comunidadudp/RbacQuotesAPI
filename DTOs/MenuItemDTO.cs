namespace RbacApi.DTOs;

public record MenuItemDTO
{
    public string Id { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string? Icon { get; set; }
    public string Route { get; set; } = default!;
    public int Order { get; set; }
    public List<MenuItemDTO> Children { get; set; } = [];
}
