using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class AuthController : ControllerBase
{
    private readonly IAuthModule authModule;
    private readonly ICurrentUser currentUser;
    private readonly IUserService userService;
    private readonly IUserMapper userMapper;

    public AuthController(IAuthModule authModule, ICurrentUser currentUser, IUserService userService, IUserMapper userMapper)
    {
        this.authModule = authModule;
        this.currentUser = currentUser;
        this.userService = userService;
        this.userMapper = userMapper;
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
        var entity = await userService.GetUserEntityByIdAsync(currentUser.GetId());
        if (entity is null) return Unauthorized();
        return Ok(userMapper.ToDto(entity));
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
