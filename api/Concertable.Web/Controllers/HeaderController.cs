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
        if (searchParams.HeaderType is null)
            return BadRequest("Search type is required.");

        var service = headerServiceFactory.Create(searchParams.HeaderType.Value);

        if (service is null)
            return BadRequest($"Invalid header type '{searchParams.HeaderType}'.");

        return Ok(await service.SearchAsync(searchParams));
    }

    [HttpGet("amount/{amount}")]
    public async Task<IActionResult> GetByAmount(int amount, [FromQuery] HeaderType? headerType)
    {
        if (headerType is null)
            return BadRequest("Header type is required.");

        var service = headerServiceFactory.Create(headerType.Value);

        if (service is null)
            return BadRequest($"Invalid header type '{headerType}'.");

        return Ok(await service.GetByAmountAsync(amount));
    }

}
