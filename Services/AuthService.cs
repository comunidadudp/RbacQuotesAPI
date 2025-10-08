using System.Security.Cryptography;
using Mapster;
using Microsoft.Extensions.Options;
using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using RbacApi.DTOs;
using RbacApi.Extensions;
using RbacApi.Infrastructure.Auth;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Validators;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IOptions<JwtConfiguration> options,
    IUserService userService,
    LoginRequestValidator loginValidator,
    RegisterRequestValidator registerValidator,
    RefreshRequestValidator refreshValidator,
    IHttpContextAccessor httpContextAccessor) : IAuthService

{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtConfiguration _jwtConfig = options.Value;
    private readonly IUserService _userService = userService;
    private readonly LoginRequestValidator _loginValidator = loginValidator;
    private readonly RegisterRequestValidator _registerValidator = registerValidator;
    private readonly RefreshRequestValidator _refreshValidator = refreshValidator;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<ApiResponseBase> GetUserinfoAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return ApiResponse.BadRequest("El nombre de usuario es obligatorio");

        var user = await _userRepository.GetByEmailAsync(username);
        if (user == null)
            return ApiResponse.BadRequest("Usuario no encontrado");

        return ApiResponse<GetUserInfoDTO>.Ok(user.Adapt<GetUserInfoDTO>());
    }

    public async Task<ApiResponseBase> LoginAsync(LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var user = await _userRepository.GetByEmailAsync(request.Username!);
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

        var stored = await _refreshTokenRepository.GetRefreshTokenAsync(request.RefreshToken!);
        if (stored == null)
            return ApiResponse.BadRequest("Token inválido (no existe)");

        // token revocado
        if (stored.Revoked)
        {
            //  tokens activos
            //var filterActive = Builders<RefreshToken>.Filter.Where(t => t.UserId == stored.UserId && !t.Revoked);
            //var updateRevoke = Builders<RefreshToken>.Update
            //    .Set(t => t.Revoked, true)
            //    .Set(t => t.ReplacedByToken, $"revoked_due_to_reuse_{DateTime.UtcNow.Ticks}");

            _httpContextAccessor.AddAuditExtraItems([
                new("entityId", $"{stored.Id}"),
                new("entityType", "RefreshToken")
            ]);

            _httpContextAccessor.AddAuditChangeItems([
                new AuditChange
                {
                    Field = nameof(RefreshToken.Revoked),
                    Before = $"{stored.Revoked}",
                    After = $"{true}"
                },
                 new AuditChange
                {
                    Field = nameof(RefreshToken.ReplacedByToken),
                    Before = stored.ReplacedByToken,
                    After = $"revoked_due_to_reuse_{DateTime.UtcNow.Ticks}"
                }
            ]);

            //await _collections.RefreshTokens.UpdateManyAsync(filterActive, updateRevoke);
            await _refreshTokenRepository.RevokeAllTokensAsync(stored.UserId);

            return ApiResponse.BadRequest("Token inválido (revocado)");
        }

        // token expiró

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            stored.Revoked = true;
            stored.ReplacedByToken = $"revoked_due_to_expiration_{DateTime.UtcNow.Ticks}";

            await _refreshTokenRepository.UpdateAsync(stored);
            return ApiResponse.Unauthorized("Token inválido (expiró)");
        }

        var user = await _userRepository.GetByIdAsync(stored.UserId);
        if (user == null)
            return ApiResponse.BadRequest("Token inválido (usuario no encontrado)");

        var newRefresh = GenerateRefreshToken(user.Id);
        await _refreshTokenRepository.AddRefreshTokenAsync(newRefresh);

        stored.Revoked = true;
        stored.ReplacedByToken = newRefresh.Token;

        await _refreshTokenRepository.UpdateAsync(stored);

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

        var existing = await _userRepository.GetByEmailAsync(request.Email!);
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

        await _userRepository.AddUserAsync(user);

        return ApiResponse<AuthResponseDTO?>.Ok(await GenerateAuthResponseForUser(user));
    }

    public async Task<ApiResponseBase> RevokeRefreshTokenAsync(RefreshRequest request)
    {
        var validation = await _refreshValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return ApiResponse.BadRequest("Error en los parámetros de entrada",
                [.. validation.Errors.Select(f => new Error(f.PropertyName, f.ErrorMessage))]);

        var stored = await _refreshTokenRepository.GetRefreshTokenAsync(t => t.Token == request.RefreshToken && !t.Revoked);
        if (stored == null)
            return ApiResponse.BadRequest("Token inválido (no existe)");

        stored.Revoked = true;
        stored.ReplacedByToken = $"revoked_due_to_expiration_{DateTime.UtcNow.Ticks}";

        await _refreshTokenRepository.UpdateAsync(stored);

        return ApiResponse<bool>.Ok(true);
    }


    // Helper
    private async Task<AuthResponseDTO> GenerateAuthResponseForUser(User user)
    {
        var permissions = await _userService.GetEffectivePermissionsForUserAsync(user);

        var access = _tokenService.GenerateAccessToken(user.Id, user.Email, user.RoleId.ToString(), permissions);
        var refresh = GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddRefreshTokenAsync(refresh);

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