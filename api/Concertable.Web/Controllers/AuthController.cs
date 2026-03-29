using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly ICurrentUser currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        this.authService = authService;
        this.currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await authService.RegisterAsync(request);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return Ok(await authService.LoginAsync(request));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        return Ok(await authService.RefreshTokenAsync(request.RefreshToken));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        return Ok(currentUser.Get());
    }
}
