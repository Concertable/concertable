using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.User.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
internal class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ICurrentUser currentUser;
    private readonly IUserMapper userMapper;

    public UserController(IUserService userService, ICurrentUser currentUser, IUserMapper userMapper)
    {
        this.userService = userService;
        this.currentUser = currentUser;
        this.userMapper = userMapper;
    }

    [HttpPut("location")]
    public async Task<ActionResult<IUser>> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var updatedUser = await userService.SaveLocationAsync(request.Latitude, request.Longitude);
        return Ok(updatedUser);
    }

    [HttpGet("/api/auth/me")]
    public async Task<ActionResult<IUser>> Me()
    {
        var entity = await userService.GetUserEntityByIdAsync(currentUser.GetId());
        if (entity is null) return Unauthorized();
        return Ok(userMapper.ToDto(entity));
    }
}
