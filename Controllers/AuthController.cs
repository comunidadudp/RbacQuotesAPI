using Microsoft.AspNetCore.Mvc;
using RbacApi.DTOs;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;


    [HttpPost("register")]
    public async Task<ApiResponseBase> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpPost("login")]
    public async Task<ApiResponseBase> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpPost("refresh")]
    public async Task<ApiResponseBase> Refresh([FromBody] RefreshRequest request)
    {
        var response = await _authService.RefreshAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpPost("revoke")]
    public async Task<ApiResponseBase> Revoke([FromBody] RefreshRequest request)
    {
        var response = await _authService.RevokeRefreshTokenAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }
}