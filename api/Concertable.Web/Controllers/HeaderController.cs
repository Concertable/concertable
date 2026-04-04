using Concertable.Application.Interfaces.Search;
using Concertable.Core.Enums;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HeaderController : ControllerBase
{
    private readonly IHeaderServiceFactory headerServiceFactory;

    public HeaderController(IHeaderServiceFactory headerServiceFactory)
    {
        this.headerServiceFactory = headerServiceFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchParams searchParams)
    {
        var service = headerServiceFactory.Create(searchParams.HeaderType!.Value);
        return Ok(await service.SearchAsync(searchParams));
    }

    [HttpGet("amount/{amount}")]
    public async Task<IActionResult> GetByAmount(int amount, [FromQuery] HeaderType? headerType)
    {
        if (headerType is null)
            return BadRequest("Header type is required.");

        var service = headerServiceFactory.Create(headerType.Value);
        return Ok(await service.GetByAmountAsync(amount));
    }

}
