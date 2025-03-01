using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.DTOs;
using Infrastructure.Services;
using Core.Exceptions;
using Microsoft.AspNetCore.Identity.Data;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await authService.Register(registerDto);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await authService.Login(loginDto);

            var role = await authService.GetFirstUserRoleAsync(user);
            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = role
            });
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
            var user = await authService.GetCurrentUserAsync();

            if (user == null) return NoContent();

            var role = await authService.GetFirstUserRoleAsync(user);
            return Ok(new UserDto()
            {
                Id = user.Id,
                Email = user.Email,
                Role = role
            });
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await authService.ConfirmEmail(userId, token);
            if (!result)
                return BadRequest(new { message = "Email confirmation failed or token is invalid" });

            return Ok(new { message = "Email confirmed successfully!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto requestDto)
        {
            return Ok(await authService.ForgotPasswordAsync(requestDto));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto requestDto)
        {
            return Ok(await authService.ResetPasswordAsync(requestDto));
        }
    }
}
