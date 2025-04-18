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
        private readonly IOwnershipService ownershipService;
        public ListingController(IListingService listingService, IOwnershipService ownershipService)
        {
            this.listingService = listingService;
            this.ownershipService = ownershipService;
        }

        [HttpGet("active/venue/{id}")]
        public async Task<ActionResult<IEnumerable<ListingDto>>> GetActiveByVenueId(int id)
        {
            return Ok(await listingService.GetActiveByVenueIdAsync(id));
        }

        [Authorize(Roles = "VenueManager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ListingDto listingDto)
        {
            await listingService.CreateAsync(listingDto);
            return Created();
        }

        [Authorize(Roles = "VenueManager")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateMultiple([FromBody] IEnumerable<ListingDto> listingsDto)
        {
            await listingService.CreateMultipleAsync(listingsDto);
            return Created();
        }

        [HttpGet("is-owner/{id}")]
        public async Task<IActionResult> IsOwner(int id)
        {
            return Ok(await ownershipService.OwnsListingAsync(id));
        }
    }
}
