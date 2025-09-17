namespace RbacApi.DTOs;

public record RegisterRequest(
    string? Fullname,
    string? Email,
    string? Password,
    string? PasswordConf
);