using Concertable.Application.Interfaces;
using Concertable.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPut("location")]
    public async Task<ActionResult<IUser>> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var updatedUser = await userService.SaveLocationAsync(request.Latitude, request.Longitude);
        return Ok(updatedUser);
    }
}
