using Core.Entities;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("all/{listingId}")]
        public async Task<ActionResult<IEnumerable<ListingApplication>>> GetAllForListingId(int listingId)
        {
            var registrations = await applicationService.GetAllForListingIdAsync(listingId);
            return Ok();
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
