using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingController : ControllerBase
    {
        private readonly IListingService listingService;
        public ListingController(IListingService listingService)
        {
            this.listingService = listingService;
        }

        [HttpGet("active/venue/{id}")]
        public async Task<ActionResult<IEnumerable<ListingDto>>> GetActiveByVenueId(int id)
        {
            return Ok(await listingService.GetActiveByVenueIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ListingDto listingDto)
        {
            listingService.Create(listingDto);
            return CreatedAtAction("", new {Id = 1});
        }
    }
}
