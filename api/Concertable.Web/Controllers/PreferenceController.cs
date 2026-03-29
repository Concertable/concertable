using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreferenceController : ControllerBase
{
    private readonly IPreferenceService preferenceService;

    public PreferenceController(IPreferenceService preferenceService)
    {
        this.preferenceService = preferenceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePreferenceRequest request)
    {
        var preference = await preferenceService.CreateAsync(request);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PreferenceDto preferenceDto)
    {
        return Ok(await preferenceService.UpdateAsync(preferenceDto));
    }

    [HttpGet("user")]
    public async Task<ActionResult<PreferenceEntity>> GetByUser()
    {
        return Ok(await preferenceService.GetByUserAsync());
    }
}
