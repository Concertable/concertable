using Concertable.Artist.Application.DTOs;
using Concertable.Artist.Application.Interfaces;
using Concertable.Concert.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Artist.Api.Controllers;

[ApiController]
[Authorize(Roles = "ArtistManager")]
[Route("api/[controller]")]
internal class ArtistDashboardController : ControllerBase
{
    private readonly IArtistService artistService;
    private readonly IConcertModule concertModule;

    public ArtistDashboardController(IArtistService artistService, IConcertModule concertModule)
    {
        this.artistService = artistService;
        this.concertModule = concertModule;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<ArtistDashboardKpisDto>> GetKpis(CancellationToken ct)
    {
        var artistId = await artistService.GetIdForCurrentUserAsync();

        var pendingTask = concertModule.GetArtistPendingApplicationsCountAsync(artistId, ct);
        var upcomingTask = concertModule.GetArtistUpcomingConcertsCountAsync(artistId, ct);

        await Task.WhenAll(pendingTask, upcomingTask);

        return Ok(new ArtistDashboardKpisDto(
            PendingApplications: pendingTask.Result,
            AcceptedAwaitingCheckout: 0,
            UpcomingConcerts: upcomingTask.Result,
            MtdPayoutsCents: 0,
            MtdPayoutsDeltaPercent: null));
    }
}
