using Concertable.Core.Parameters;
using Concertable.Search.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/concert/headers")]
public class ConcertHeaderController : ControllerBase
{
    private readonly IConcertHeaderModule concertHeaderModule;

    public ConcertHeaderController(IConcertHeaderModule concertHeaderModule)
    {
        this.concertHeaderModule = concertHeaderModule;
    }

    [AllowAnonymous]
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular()
        => Ok(await concertHeaderModule.GetPopularAsync());

    [AllowAnonymous]
    [HttpGet("free")]
    public async Task<IActionResult> GetFree()
        => Ok(await concertHeaderModule.GetFreeAsync());

    [HttpGet("recommended")]
    [Authorize]
    public async Task<IActionResult> GetRecommended([FromQuery] ConcertParams concertParams)
        => Ok(await concertHeaderModule.GetRecommendedAsync(concertParams));
}
