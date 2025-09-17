using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RbacApi.Infrastructure.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string username, string role,
        IEnumerable<string>? permissions = null,
        int expireMinutes = 120);

    (ClaimsPrincipal? principal, JwtSecurityToken? token) ValidateToken(string token, bool validateLifetime = true);
}