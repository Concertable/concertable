using Application.DTOs;
using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Web.Controllers;

/// <summary>
/// Auth gateway for the new User table (OAuth refactor). Will replace AuthController once we migrate off Identity.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppAuthController : ControllerBase
{
    private readonly IAppAuthService _appAuthService;
    private readonly IAppTokenService _appTokenService;
    private readonly AppAuthSettings _appAuthSettings;

    public AppAuthController(IAppAuthService appAuthService, IAppTokenService appTokenService, IOptions<AppAuthSettings> appAuthSettings)
    {
        _appAuthService = appAuthService;
        _appTokenService = appTokenService;
        _appAuthSettings = appAuthSettings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _appAuthService.RegisterAsync(request);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AppLoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _appAuthService.LoginAsync(request);
        var accessToken = _appTokenService.CreateToken(user.Id, user.Email, user.Role ?? string.Empty);
        var expiresInSeconds = _appAuthSettings.AccessTokenExpirationMinutes * 60;
        return Ok(new AppLoginResponse(user, accessToken, expiresInSeconds));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _appAuthService.LogoutAsync();
        return NoContent();
    }
}
