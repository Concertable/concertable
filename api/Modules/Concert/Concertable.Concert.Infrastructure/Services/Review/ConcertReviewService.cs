using Concertable.Concert.Application.Interfaces.Reviews;
using Concertable.Concert.Application.Mappers;
using Concertable.Concert.Application.Requests;
using Concertable.Identity.Contracts;
using Concertable.Shared;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Review;

internal class ConcertReviewService(
    IConcertReviewRepository reviewRepository,
    ITicketRepository ticketRepository,
    IReviewValidator reviewValidator,
    ICurrentUser currentUser) : IConcertReviewService
{
    public Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams) =>
        reviewRepository.GetByConcertAsync(concertId, pageParams);

    public Task<ReviewSummaryDto> GetSummaryAsync(int concertId) =>
        reviewRepository.GetSummaryByConcertAsync(concertId);

    public Task<bool> CanCurrentUserReviewAsync(int concertId) =>
        reviewValidator.CanUserReviewConcertAsync(currentUser.GetId(), concertId);

    public async Task<ReviewDto> CreateAsync(CreateReviewRequest request)
    {
        var userId = currentUser.GetId();
        var ticket = await ticketRepository.GetByUserIdAndConcertIdAsync(userId, request.ConcertId)
            ?? throw new NotFoundException("Cannot find ticket");

        var application = ticket.Concert.Booking.Application;
        var review = ReviewEntity.Create(
            ticket.Id,
            request.Stars,
            request.Details,
            application.ArtistId,
            application.Opportunity.VenueId,
            ticket.ConcertId);

        await reviewRepository.AddAsync(review);
        await reviewRepository.SaveChangesAsync();

        return review.ToDto(currentUser.Email ?? string.Empty);
    }
}
