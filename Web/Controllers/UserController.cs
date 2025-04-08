using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
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

        [HttpPut("{id}/location")]
        public async Task<ActionResult<UserDto>> UpdateLocation(int id, [FromBody] UpdateLocationRequest request)
        {
            var updatedUser = await userService.UpdateLocationAsync(request.Latitude, request.Longitude);
            return Ok(updatedUser);
        }
    }
}
