using System.Reflection;
using System.Text;
using Amazon.S3;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Interfaces;
using RbacApi.Data.Repositories;
using RbacApi.Handlers;
using RbacApi.Infrastructure.Auth;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Services;
using RbacApi.Infrastructure.Storage.AWS;
using RbacApi.Mapping;
using RbacApi.Pagination;
using RbacApi.Providers;
using RbacApi.Services;
using RbacApi.Services.Interfaces;

namespace RbacApi.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguration>(configuration.GetSection(nameof(JwtConfiguration)));
        services.Configure<S3BucketOptions>(configuration.GetSection(nameof(S3BucketOptions)));
        services.Configure<CloudFrontConfig>(configuration.GetSection(nameof(CloudFrontConfig)));
        services.Configure<PaginationOptions>(configuration.GetSection(nameof(PaginationOptions)));
        
        return services;
    }

    public static IServiceCollection AddMongoDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("QuotesDB") ?? "mongo:db//localhost:27017";
        var mongoDbName = configuration.GetValue<string>("MongoDbName") ?? "quotes";
        var client = new MongoClient(mongoConnectionString);

        services.AddSingleton(client.GetDatabase(mongoDbName));
        services.AddSingleton<CollectionsProvider>();

        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

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

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddSingleton<ITokenService, BearerTokenService>();

        services.AddSingleton<IAuditQueue, AuditQueue>();
        services.AddHostedService<AuditWriterBackgroundService>();

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonS3>();

        services.AddSingleton<IStorageService, S3StorageService>();
        services.AddSingleton<ISigner, CloudFrontUrlSigner>();

        // Mapster DI
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}