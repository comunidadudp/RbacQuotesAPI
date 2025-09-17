namespace RbacApi.DTOs;

public record LoginRequest(
    string? Username,
    string? Password
);
