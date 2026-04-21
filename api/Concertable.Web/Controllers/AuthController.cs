using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthModule authModule;
    private readonly ICurrentUser currentUser;

    public AuthController(IAuthModule authModule, ICurrentUser currentUser)
    {
        this.authModule = authModule;
        this.currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await authModule.RegisterAsync(request);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return Ok(await authModule.LoginAsync(request));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await authModule.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        return Ok(await authModule.RefreshTokenAsync(request.RefreshToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<IUser>> Me()
    {
        var user = await authModule.GetCurrentUserAsync(currentUser.GetId());
        if (user is null) return Unauthorized();
        return Ok(user);
    }

    [HttpPost("send-verification")]
    public async Task<IActionResult> SendVerification([FromBody] ForgotPasswordRequest request)
    {
        await authModule.SendVerificationEmailAsync(request.Email);
        return NoContent();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        await authModule.VerifyEmailAsync(request.Token);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authModule.ForgotPasswordAsync(request.Email);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await authModule.ResetPasswordAsync(request);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await authModule.ChangePasswordAsync(currentUser.GetId(), request);
        return NoContent();
    }
}