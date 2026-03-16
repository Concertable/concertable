using Application.DTOs;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService accountService;
    private readonly ICurrentUser currentUser;

    public AccountController(IAccountService accountService, ICurrentUser currentUser)
    {
        this.accountService = accountService;
        this.currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await accountService.RegisterAsync(request);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return Ok(await accountService.LoginAsync(request));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await accountService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        return Ok(await accountService.RefreshTokenAsync(request.RefreshToken));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        return Ok(currentUser.Get());
    }
}
