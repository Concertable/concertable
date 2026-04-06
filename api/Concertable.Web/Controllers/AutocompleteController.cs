using Concertable.Application.Interfaces.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AutocompleteController : ControllerBase
{
    private readonly IHeaderAutocompleteService headerAutocompleteService;

    public AutocompleteController(IHeaderAutocompleteService headerAutocompleteService)
    {
        this.headerAutocompleteService = headerAutocompleteService;
    }

    [HttpGet("headers")]
    public async Task<IActionResult> GetHeaders([FromQuery] string? searchTerm)
        => Ok(await headerAutocompleteService.GetAsync(searchTerm));
}
