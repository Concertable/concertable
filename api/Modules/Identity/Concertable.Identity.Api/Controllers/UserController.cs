using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Identity.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
internal class UsersController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ICurrentUser currentUser;

    public UsersController(IUserService userService, ICurrentUser currentUser)
    {
        this.userService = userService;
        this.currentUser = currentUser;
    }

    [HttpPut("location")]
    public async Task<ActionResult<IUser>> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var updatedUser = await userService.SaveLocationAsync(request.Latitude, request.Longitude);
        return Ok(updatedUser);
    }
}
