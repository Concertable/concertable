using Core.Entities;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Responses;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingApplicationController : ControllerBase
    {
        private readonly IListingApplicationService listingApplicationService;
        private readonly IListingApplicationValidationService applicationValidationService;
        private readonly IArtistService artistService;

        public ListingApplicationController(
            IListingApplicationService listingApplicationService, 
            IListingApplicationValidationService eventSchedulingService,
            IArtistService artistService
            )
        {
            this.listingApplicationService = listingApplicationService;
            this.applicationValidationService = applicationValidationService;
            this.artistService = artistService;
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

        [Authorize(Roles = "ArtistManager")]
        [HttpGet("can-apply/{listingId}")]
        public async Task<ActionResult<bool>> CanApplyForListing(int listingId)
        {
            var artist = await artistService.GetDetailsForCurrentUserAsync();

            if (artist is null)
                return NotFound("Artist not found");

            var response = await applicationValidationService.CanApplyForListingAsync(listingId, artist.Id);

            if (!response.IsValid)
                return BadRequest(response.Reason);

            return Ok(true);
        }

        [Authorize(Roles = "VenueManager")]
        [HttpGet("can-accept/{applicationId}")]
        public async Task<ActionResult<bool>> CanAcceptApplication(int applicationId)
        {
            var result = await applicationValidationService.CanAcceptListingApplicationAsync(applicationId);

            if (!result.IsValid)
                return BadRequest(result.Reason);

            return Ok(true);
        }


    }
}
