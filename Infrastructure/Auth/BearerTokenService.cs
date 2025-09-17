using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RbacApi.Infrastructure.Interfaces;

namespace RbacApi.Infrastructure.Auth;

public class BearerTokenService : ITokenService
{
    private readonly JwtConfiguration _jwtConfiguration;
    private readonly byte[] _key;

    public BearerTokenService(IOptions<JwtConfiguration> config)
    {
        _jwtConfiguration = config.Value;
        _key = Encoding.UTF8.GetBytes(_jwtConfiguration.Key) ?? throw new ArgumentNullException("JwtConfiguration.Key is required");
    }

    public string GenerateAccessToken(string userId, string username, string role,
        IEnumerable<string>? permissions = null, int expireMinutes = 120)
    {
        List<Claim> claims = [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        ];

        if (permissions != null)
        {
            foreach (var p in permissions)
                claims.Add(new Claim("permissions", p));
        }

        var securityKey = new SymmetricSecurityKey(_key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfiguration.Issuer,
            audience: _jwtConfiguration.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfiguration.AccessTokenMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (ClaimsPrincipal? principal, JwtSecurityToken? token) ValidateToken(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfiguration.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateLifetime = true,
                LifetimeValidator = JwtValidator.CustomLifeTimeValidator,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out var securityToken);

            return (principal, securityToken as JwtSecurityToken);
        }
        catch
        {
            return (null, null);
        }
    }
}
