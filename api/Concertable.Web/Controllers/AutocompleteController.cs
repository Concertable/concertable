using Concertable.Search.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AutocompleteController : ControllerBase
{
    private readonly IAutocompleteModule autocompleteModule;

    public AutocompleteController(IAutocompleteModule autocompleteModule)
    {
        this.autocompleteModule = autocompleteModule;
    }

    [HttpGet("headers")]
    public async Task<IActionResult> GetHeaders([FromQuery] string? searchTerm)
        => Ok(await autocompleteModule.GetAsync(searchTerm));
}
