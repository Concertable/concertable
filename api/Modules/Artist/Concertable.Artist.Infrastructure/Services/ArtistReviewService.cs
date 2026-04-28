using Concertable.Artist.Application.Interfaces;
using Concertable.Artist.Infrastructure.Data;
using Concertable.Artist.Infrastructure.Mappers;
using Concertable.Concert.Contracts;
using Concertable.User.Contracts;
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
        return projection.ToReviewSummaryDto();
    }

    public Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams) =>
        concertModule.GetReviewsByArtistAsync(artistId, pageParams);

    public Task<bool> CanCurrentUserReviewAsync(int artistId) =>
        concertModule.CanUserReviewArtistAsync(currentUser.GetId(), artistId);
}
