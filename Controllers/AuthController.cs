using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.DTOs;
using RbacApi.Filters;
using RbacApi.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;


    [HttpPost("register")]
    [Audit("auth.register")]
    public async Task<ApiResponseBase> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpPost("login")]
    [Audit("auth.login")]
    public async Task<ApiResponseBase> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpPost("refresh")]
    [Audit("auth.refresh")]
    public async Task<ApiResponseBase> Refresh([FromBody] RefreshRequest request)
    {
        var response = await _authService.RefreshAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpPost("revoke")]
    [Audit("auth.revoke")]
    public async Task<ApiResponseBase> Revoke([FromBody] RefreshRequest request)
    {
        var response = await _authService.RevokeRefreshTokenAsync(request);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }

    [HttpGet("user")]
    [Audit("auth.userinfo")]
    [Authorize]
    public async Task<ApiResponseBase> UserInfo()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var response = await _authService.GetUserinfoAsync(username!);
        HttpContext.Response.StatusCode = response.Status;
        return response;
    }
}