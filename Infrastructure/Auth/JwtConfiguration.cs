namespace RbacApi.Infrastructure.Auth;

public class JwtConfiguration
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; }
    public int RefreshTokenDays { get; init; }
}