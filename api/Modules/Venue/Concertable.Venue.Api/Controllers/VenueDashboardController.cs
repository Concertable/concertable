using Concertable.Concert.Contracts;
using Concertable.Venue.Application.DTOs;
using Concertable.Venue.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Venue.Api.Controllers;

[ApiController]
[Authorize(Roles = "VenueManager")]
[Route("api/[controller]")]
internal class VenueDashboardController : ControllerBase
{
    private readonly IVenueService venueService;
    private readonly IConcertModule concertModule;

    public VenueDashboardController(IVenueService venueService, IConcertModule concertModule)
    {
        this.venueService = venueService;
        this.concertModule = concertModule;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<VenueDashboardKpisDto>> GetKpis(CancellationToken ct)
    {
        var venueId = await venueService.GetIdForCurrentUserAsync();

        var applicationsTask = concertModule.GetVenueApplicationsAwaitingReviewCountAsync(venueId, ct);
        var openOpportunitiesTask = concertModule.GetVenueOpenOpportunitiesCountAsync(venueId, ct);
        var upcomingConcertsTask = concertModule.GetVenueUpcomingConcertsCountAsync(venueId, ct);

        await Task.WhenAll(applicationsTask, openOpportunitiesTask, upcomingConcertsTask);

        return Ok(new VenueDashboardKpisDto(
            ApplicationsToReview: applicationsTask.Result,
            ApplicationsToReviewDelta: null,
            OpenOpportunities: openOpportunitiesTask.Result,
            UpcomingConcerts: upcomingConcertsTask.Result,
            MtdRevenueCents: 0,
            MtdRevenueDeltaPercent: null));
    }
}
