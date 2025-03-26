using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Web.Controllers
{
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
        public async Task<IActionResult> Create([FromBody]CreatePreferenceDto createPreferenceDto)
        {
            var preference = await preferenceService.CreateAsync(createPreferenceDto);
            return Created();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]PreferenceDto preferenceDto)
        {
            return Ok(await preferenceService.UpdateAsync(preferenceDto));
        }

        [HttpGet("user")]
        public async Task<ActionResult<Preference>> GetByUser()
        {
            return Ok(await preferenceService.GetByUserAsync());
        }
    }
}
