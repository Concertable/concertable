using Concertable.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreController : ControllerBase
{
    [HttpGet("all")]
    public ActionResult<IEnumerable<Genre>> GetAll() => Ok(Enum.GetValues<Genre>());
}
