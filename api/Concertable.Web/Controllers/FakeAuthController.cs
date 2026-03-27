using Application.Interfaces.Auth;
using Concertable.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FakeAuthController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly ITokenService tokenService;
    private readonly IConfiguration configuration;

    public FakeAuthController(ApplicationDbContext context, ITokenService tokenService, IConfiguration configuration)
    {
        this.context = context;
        this.tokenService = tokenService;
        this.configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string email)
    {
        if (!configuration.GetValue<bool>("UseFakeExternalServices"))
            return NotFound();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return NotFound($"No user found with email {email}");

        var token = tokenService.CreateAccessToken(user.Id, user.Email!, user.Role);

        return Ok(new { token });
    }
}
