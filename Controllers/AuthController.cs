using Microsoft.AspNetCore.Mvc;
using RbacApi.DTOs;
using RbacApi.Services.Interfaces;

namespace RbacApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        return Ok(await _authService.RegisterAsync(request));
    }
}