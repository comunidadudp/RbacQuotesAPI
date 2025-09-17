using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RbacApi.Data;
using RbacApi.Infrastructure.Auth;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Services;
using RbacApi.Services.Interfaces;

namespace RbacApi.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguration>(configuration.GetSection(nameof(JwtConfiguration)));
        return services;
    }

    public static IServiceCollection AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("QuotesDB") ?? "mongo:db//localhost:27017";
        var mongoDbName = configuration.GetValue<string>("MongoDbName") ?? "quotes";
        services.AddDbContext<QuotesDbContext>(options =>
        {
            options.UseMongoDB(mongoConnectionString, mongoDbName);
        });

        return services;
    }

    public static IServiceCollection AddBearerAuthenticationScheme(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                JwtConfiguration jwtConfig = configuration.GetSection(nameof(JwtConfiguration)).Get<JwtConfiguration>()!;
                var key = Encoding.UTF8.GetBytes(jwtConfig.Key);

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfig.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    LifetimeValidator = JwtValidator.CustomLifeTimeValidator,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITokenService, BearerTokenService>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}