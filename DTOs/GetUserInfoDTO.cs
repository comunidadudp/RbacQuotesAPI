namespace RbacApi.DTOs;

public record GetUserInfoDTO
(
    string Id,
    string Fullname,
    string Email,
    string Role,
    List<string> Permissions,
    bool Active,
    string CreatedAt
);