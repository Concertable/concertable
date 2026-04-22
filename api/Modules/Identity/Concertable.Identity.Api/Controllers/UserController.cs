using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Identity.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
internal class UsersController : ControllerBase
{
    private readonly IAuthModule authModule;
    private readonly ICurrentUser currentUser;

    public UsersController(IAuthModule authModule, ICurrentUser currentUser)
    {
        this.authModule = authModule;
        this.currentUser = currentUser;
    }

    [HttpPut("location")]
    public async Task<ActionResult<IUser>> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var updatedUser = await authModule.SaveLocationAsync(currentUser.GetId(), request.Latitude, request.Longitude);
        return Ok(updatedUser);
    }
}
