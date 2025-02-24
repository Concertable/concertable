using Core.Entities;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingApplicationController : ControllerBase
    {
        private readonly IListingApplicationService listingApplicationService;

        public ListingApplicationController(IListingApplicationService listingApplicationService)
        {
            this.listingApplicationService = listingApplicationService;
        }

        [Authorize(Roles = "VenueManager")]
        [HttpGet("all/{id}")]
        public async Task<ActionResult<IEnumerable<ListingApplicationDto>>> GetAllForListingId(int id)
        {
            return Ok(await listingApplicationService.GetAllForListingIdAsync(id));
        }

        [Authorize(Roles = "ArtistManager")]
        [HttpPost("{listingId}")]
        public async Task<IActionResult> ApplyForListing(int listingId)
        {
            await listingApplicationService.ApplyForListingAsync(listingId);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ListingApplication>> GetById(int id)
        {
            return Ok(await listingApplicationService.GetByIdAsync(id));
        }
    }
}
