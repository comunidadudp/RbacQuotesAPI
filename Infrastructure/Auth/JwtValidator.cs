using Microsoft.IdentityModel.Tokens;

namespace RbacApi.Infrastructure.Auth;

public static class JwtValidator
{
    public static bool CustomLifeTimeValidator(
        DateTime? notBefore,
        DateTime? expires,
        SecurityToken securityToken,
        TokenValidationParameters validationParameters
    )
    {
        if (expires != null && expires < DateTime.UtcNow)
        {
            return false;
        }

        if (notBefore != null && notBefore > DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }
}