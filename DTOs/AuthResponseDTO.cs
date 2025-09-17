namespace RbacApi.DTOs;

public record AuthResponseDTO(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string Role,
    IEnumerable<string>? Permissions
);