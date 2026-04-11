using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Core.Exceptions;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository reviewRepository;
    private readonly ITicketRepository ticketRepository;
    private readonly ICurrentUser currentUser;

    public ReviewService(
        IReviewRepository reviewRepository,
        ITicketRepository ticketRepository,
        ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.ticketRepository = ticketRepository;
        this.currentUser = currentUser;
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewRequest request)
    {
        var review = request.ToEntity();

        var userId = currentUser.GetId();
        var ticket = await ticketRepository.GetByUserIdAndConcertIdAsync(userId, request.ConcertId)
            ?? throw new NotFoundException("Cannot find ticket");

        review.TicketId = ticket.Id;

        await reviewRepository.AddAsync(review);
        await reviewRepository.SaveChangesAsync();

        return review.ToDto();
    }

    public Task<IPagination<ReviewDto>> GetByArtistIdAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetByArtistIdAsync(id, pageParams);

    public Task<IPagination<ReviewDto>> GetByConcertIdAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetByConcertIdAsync(id, pageParams);

    public Task<IPagination<ReviewDto>> GetByVenueIdAsync(int id, IPageParams pageParams) =>
        reviewRepository.GetByVenueIdAsync(id, pageParams);

    public async Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id) =>
        await reviewRepository.GetSummaryByArtistIdAsync(id);

    public async Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id) =>
        await reviewRepository.GetSummaryByConcertIdAsync(id);

    public async Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id) =>
        await reviewRepository.GetSummaryByVenueIdAsync(id);
}
