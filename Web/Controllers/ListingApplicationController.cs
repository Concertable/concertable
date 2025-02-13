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
        private readonly IListingApplicationService applicationService;

        public ListingApplicationController(IListingApplicationService applicationService)
        {
            this.applicationService = applicationService;
        }

        [Authorize(Roles = "VenueManager")]
        [HttpGet("all/{id}")]
        public async Task<ActionResult<IEnumerable<ListingApplicationDto>>> GetAllForListingId(int id)
        {
            return Ok(await applicationService.GetAllForListingIdAsync(id));
        }

        [Authorize(Roles = "ArtistManager")]
        [HttpPost("{listingId}")]
        public async Task<IActionResult> ApplyForListing(int listingId)
        {
            await applicationService.ApplyForListingAsync(listingId);
            return NoContent();
        }
    }
}
