using Application.Interfaces.Search;
using Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/concert/headers")]
[AllowAnonymous]
public class ConcertHeaderController : ControllerBase
{
    private readonly IConcertHeaderService concertHeaderService;

    public ConcertHeaderController(IConcertHeaderService concertHeaderService)
    {
        this.concertHeaderService = concertHeaderService;
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular()
        => Ok(await concertHeaderService.GetPopularAsync());

    [HttpGet("free")]
    public async Task<IActionResult> GetFree()
        => Ok(await concertHeaderService.GetFreeAsync());

    [HttpGet("recommended")]
    [Authorize]
    public async Task<IActionResult> GetRecommended([FromQuery] ConcertParams concertParams)
        => Ok(await concertHeaderService.GetRecommendedAsync(concertParams));
}
