using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Review;

internal class ConcertReviewService : IReviewService
{
    private readonly IConcertReviewRepository reviewRepository;
    private readonly ITicketRepository ticketRepository;
    private readonly ICurrentUser currentUser;

    public ConcertReviewService(
        IConcertReviewRepository reviewRepository,
        ITicketRepository ticketRepository,
        ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.ticketRepository = ticketRepository;
        this.currentUser = currentUser;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetAsync(id, pageParams);

    public Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
        reviewRepository.GetSummaryAsync(id);

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
