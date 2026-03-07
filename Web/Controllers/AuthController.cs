using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.DTOs;
using Infrastructure.Services;
using Core.Exceptions;
using Application.Requests;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly ICurrentUserService currentUserService;

        public AuthController(IAuthService authService, ICurrentUserService currentUserService)
        {
            this.authService = authService;
            this.currentUserService = currentUserService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await authService.Register(request);

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
        {
            return Ok(await authService.Login(request));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await authService.Logout();
            return NoContent();
        }

        [Authorize]
        [HttpGet("current-user")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            return Ok(await currentUserService.GetAsync());
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await authService.ConfirmEmailAsync(userId, token);
            if (!result)
                return BadRequest(new { message = "Email confirmation failed or token is invalid" });

            return Ok(new { message = "Email confirmed successfully!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            return Ok(await authService.ForgotPasswordAsync(request));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            return Ok(await authService.ResetPasswordAsync(request));
        }

        [Authorize]
        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange([FromBody] ChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await authService.RequestEmailChangeAsync(request.NewEmail);
            return Ok(new { message = "A confirmation link has been sent to your current email." });
        }

        [HttpGet("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token, [FromQuery] string newEmail)
        {
            var result = await authService.ConfirmEmailChangeAsync(token, newEmail);

            return Ok(new { message = "Your email has been successfully updated!" });
        }


    }
}
