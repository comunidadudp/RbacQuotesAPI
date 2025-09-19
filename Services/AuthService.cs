using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.DTOs;
using RbacApi.Infrastructure.Auth;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Validators;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public sealed class AuthService(
    CollectionsProvider collections,
    ITokenService tokenService,
    IOptions<JwtConfiguration> options,
    IUserService userService,
    LoginRequestValidator loginValidator,
    RegisterRequestValidator registerValidator,
    RefreshRequestValidator refreshValidator) : IAuthService

{
    private readonly CollectionsProvider _collections = collections;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtConfiguration _jwtConfig = options.Value;
    private readonly IUserService _userService = userService;
    private readonly LoginRequestValidator _loginValidator = loginValidator;
    private readonly RegisterRequestValidator _registerValidator = registerValidator;
    private readonly RefreshRequestValidator _refreshValidator = refreshValidator;


    public async Task<ApiResponseBase> LoginAsync(LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var user = await _collections.Users.Find(u => u.Email == request.Username).FirstOrDefaultAsync();
        if (user == null)
            return ApiResponse.BadRequest("Usuario y/o contraseña incorrectos");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse.BadRequest("Usuario y/o contraseña incorrectos");

        return ApiResponse<AuthResponseDTO>.Ok(await GenerateAuthResponseForUser(user));
    }

    public async Task<ApiResponseBase> RefreshAsync(RefreshRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var stored = await _collections.RefreshTokens.Find(t => t.Token == request.RefreshToken).FirstOrDefaultAsync();
        if (stored == null)
            return ApiResponse.BadRequest("Token inválido (no existe)");

        // token revocado
        if (stored.Revoked)
        {
            //  tokens activos
            var filterActive = Builders<RefreshToken>.Filter.Where(t => t.UserId == stored.UserId && !t.Revoked);
            var updateRevoke = Builders<RefreshToken>.Update
                .Set(t => t.Revoked, true)
                .Set(t => t.ReplacedByToken, $"revoked_due_to_reuse_{DateTime.UtcNow.Ticks}");

            await _collections.RefreshTokens.UpdateManyAsync(filterActive, updateRevoke);

            return ApiResponse.BadRequest("Token inválido (revocado)");
        }

        // token expiró

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            var updateExp = Builders<RefreshToken>.Update
                .Set(t => t.Revoked, true)
                .Set(t => t.ReplacedByToken, $"revoked_due_to_expiration_{DateTime.UtcNow.Ticks}");

            await _collections.RefreshTokens.UpdateOneAsync(t => t.Id == stored.Id, updateExp);
            return ApiResponse.Unauthorized("Token inválido (expiró)");
        }

        var user = await _collections.Users.Find(u => u.Id == stored.UserId).FirstOrDefaultAsync();
        if (user == null)
            return ApiResponse.BadRequest("Token inválido (usuario no encontrado)");

        var newRefresh = GenerateRefreshToken(user.Id);
        await _collections.RefreshTokens.InsertOneAsync(newRefresh);

        stored.Revoked = true;
        stored.ReplacedByToken = newRefresh.Token;

        var updateStored = Builders<RefreshToken>.Update
                .Set(t => t.Revoked, true)
                .Set(t => t.ReplacedByToken, newRefresh.Token);

        await _collections.RefreshTokens.UpdateOneAsync(t => t.Id == stored.Id, updateStored);

        var permissions = await _userService.GetEffectivePermissionsForUserAsync(user);
        var access = _tokenService.GenerateAccessToken(user.Id, user.Email, user.RoleId.ToString(), permissions, _jwtConfig.AccessTokenMinutes);

        return ApiResponse<AuthResponseDTO?>.Ok(new AuthResponseDTO
        (
            AccessToken: access,
            RefreshToken: newRefresh.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenMinutes),
            UserId: user.Id,
            Email: user.Email,
            Role: user.RoleId.ToString(),
            Permissions: permissions
        ));
    }

    public async Task<ApiResponseBase> RegisterAsync(RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var existing = await _collections.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (existing != null)
            return ApiResponse.BadRequest("El usuario ya existe");

        var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Fullname = request.Fullname!,
            Email = request.Email!,
            PasswordHash = hashed,
            RoleId = Enums.RoleType.None,
            Permissions = [],
            Active = true
        };

        await _collections.Users.InsertOneAsync(user);

        return ApiResponse<AuthResponseDTO?>.Ok(await GenerateAuthResponseForUser(user));
    }

    public async Task<ApiResponseBase> RevokeRefreshTokenAsync(RefreshRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var stored = await _collections.RefreshTokens.Find(t => t.Token == request.RefreshToken && !t.Revoked).FirstOrDefaultAsync();
        if (stored == null)
            return ApiResponse.BadRequest("Token inválido (no existe)");

        var updateStored = Builders<RefreshToken>.Update
                .Set(t => t.Revoked, true)
                .Set(t => t.ReplacedByToken, $"revoked_ok_{DateTime.UtcNow.Ticks}");

        await _collections.RefreshTokens.UpdateOneAsync(t => t.Id == stored.Id, updateStored);

        return ApiResponse<bool>.Ok(true);
    }


    // Helper
    private async Task<AuthResponseDTO> GenerateAuthResponseForUser(User user)
    {
        var permissions = await _userService.GetEffectivePermissionsForUserAsync(user);

        var access = _tokenService.GenerateAccessToken(user.Id, user.Email, user.RoleId.ToString(), permissions);
        var refresh = GenerateRefreshToken(user.Id);

        await _collections.RefreshTokens.InsertOneAsync(refresh);

        return new AuthResponseDTO
        (
            AccessToken: access,
            RefreshToken: refresh.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenMinutes),
            UserId: user.Id,
            Email: user.Email,
            Role: user.RoleId.ToString(),
            Permissions: permissions
        );
    }

    private RefreshToken GenerateRefreshToken(string userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenDays),
        };
    }
}