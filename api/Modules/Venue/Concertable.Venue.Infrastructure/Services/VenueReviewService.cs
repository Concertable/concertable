using Concertable.Concert.Contracts;
using Concertable.Identity.Contracts;
using Concertable.Shared;
using Concertable.Venue.Application.Interfaces;
using Concertable.Venue.Infrastructure.Data;
using Concertable.Venue.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Venue.Infrastructure.Services;

internal class VenueReviewService(
    VenueDbContext context,
    IConcertModule concertModule,
    ICurrentUser currentUser) : IVenueReviewService
{
    public async Task<ReviewSummaryDto> GetSummaryAsync(int venueId)
    {
        var projection = await context.VenueRatingProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.VenueId == venueId);
        return projection.ToReviewSummaryDto();
    }

    public Task<IPagination<ReviewDto>> GetAsync(int venueId, IPageParams pageParams) =>
        concertModule.GetReviewsByVenueAsync(venueId, pageParams);

    public Task<bool> CanCurrentUserReviewAsync(int venueId) =>
        concertModule.CanUserReviewVenueAsync(currentUser.GetId(), venueId);
}
