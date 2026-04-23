using Concertable.Artist.Application.Interfaces;
using Concertable.Artist.Infrastructure.Data;
using Concertable.Concert.Contracts;
using Concertable.Identity.Contracts;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Services;

internal class ArtistReviewService(
    ArtistDbContext context,
    IConcertModule concertModule,
    ICurrentUser currentUser) : IArtistReviewService
{
    public async Task<ReviewSummaryDto> GetSummaryAsync(int artistId)
    {
        var projection = await context.ArtistRatingProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ArtistId == artistId);
        return projection is null
            ? new ReviewSummaryDto(0, null)
            : new ReviewSummaryDto(projection.ReviewCount, projection.AverageRating);
    }

    public Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams) =>
        concertModule.GetReviewsByArtistAsync(artistId, pageParams);

    public Task<bool> CanCurrentUserReviewAsync(int artistId) =>
        concertModule.CanUserReviewArtistAsync(currentUser.GetId(), artistId);
}
