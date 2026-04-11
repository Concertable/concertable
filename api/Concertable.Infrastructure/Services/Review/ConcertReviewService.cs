using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Mappers;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Core.Exceptions;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Services.Review;

public class ConcertReviewService : IReviewService
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
        var review = request.ToEntity();

        var userId = currentUser.GetId();
        var ticket = await ticketRepository.GetByUserIdAndConcertIdAsync(userId, request.ConcertId)
            ?? throw new NotFoundException("Cannot find ticket");

        review.TicketId = ticket.Id;

        await reviewRepository.AddAsync(review);
        await reviewRepository.SaveChangesAsync();

        return review.ToDto();
    }
}
