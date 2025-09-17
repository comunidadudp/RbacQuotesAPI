using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.DTOs;
using RbacApi.Infrastructure.Auth;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public sealed class AuthService(QuotesDbContext context, ITokenService tokenService, IOptions<JwtConfiguration> options)
    : IAuthService
{
    private readonly QuotesDbContext _context = context;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtConfiguration _jwtConfig = options.Value;

    public Task<AuthResponseDTO> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterRequest request)
    {
        var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing != null)
            return null;

        var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Fullname = request.Fullname,
            Email = request.Email,
            PasswordHash = hashed,
            RoleId = Enums.RoleType.None,
            Permissions = [],
            Active = true
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return GenerateAuthResponseForUser(user);
    }


    // Helper
    private AuthResponseDTO GenerateAuthResponseForUser(User user)
    {
        IEnumerable<string> permissions = [];

        var access = _tokenService.GenerateAccessToken(user.Id, user.Email, user.RoleId.ToString(), permissions);

        return new AuthResponseDTO
        (
            AccessToken: access,
            RefreshToken: "",
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenMinutes),
            UserId: user.Id,
            Email: user.Email,
            Role: user.RoleId.ToString(),
            Permissions: permissions
        );
    }
}