using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Requests;
using Core.Exceptions;
using Core.Parameters;
using Application.Responses;
using Core.Interfaces;

namespace Infrastructure.Services;

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

    public async Task<Pagination<ReviewDto>> GetByArtistIdAsync(int id, IPageParams pageParams)
    {
        var reviews = await reviewRepository.GetByArtistIdAsync(id, pageParams);
        return new Pagination<ReviewDto>(reviews.Data.ToDtos(), reviews.TotalCount, reviews.PageNumber, reviews.PageSize);
    }

    public async Task<Pagination<ReviewDto>> GetByConcertIdAsync(int id, IPageParams pageParams)
    {
        var reviews = await reviewRepository.GetByConcertIdAsync(id, pageParams);
        return new Pagination<ReviewDto>(reviews.Data.ToDtos(), reviews.TotalCount, reviews.PageNumber, reviews.PageSize);
    }

    public async Task<Pagination<ReviewDto>> GetByVenueIdAsync(int id, IPageParams pageParams)
    {
        var reviews = await reviewRepository.GetByVenueIdAsync(id, pageParams);
        return new Pagination<ReviewDto>(reviews.Data.ToDtos(), reviews.TotalCount, reviews.PageNumber, reviews.PageSize);
    }

    public async Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id) =>
        await reviewRepository.GetSummaryByArtistIdAsync(id);

    public async Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id) =>
        await reviewRepository.GetSummaryByConcertIdAsync(id);

    public async Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id) =>
        await reviewRepository.GetSummaryByVenueIdAsync(id);
}
