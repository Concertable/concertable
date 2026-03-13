using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Requests;
using Core.Entities;
using Core.Exceptions;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Helpers;

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

    public async Task SetAverageRatingAsync(VenueDto venue)
    {
        if (venue is not null)
            venue.Rating = await reviewRepository.GetAverageRatingByVenueIdAsync(venue.Id);
    }

    public async Task<ReviewDto> CreateAsync(CreateReviewRequest request)
    {
        var review = request.ToEntity();

        var userId = currentUser.GetId();
        var ticket = await ticketRepository.GetByUserIdAndConcertIdAsync(userId, request.ConcertId);

        if (ticket is null)
            throw new NotFoundException("Cannot find ticket");

        review.TicketId = ticket.Id;

        await reviewRepository.AddAsync(review);
        await reviewRepository.SaveChangesAsync();

        return review.ToDto();
    }

    private async Task<Pagination<ReviewDto>> GetAsync(
        Func<IPageParams, Task<Pagination<Review>>> getAsync,
        IPageParams pageParams)
    {
        var reviews = await getAsync(pageParams);

        return new Pagination<ReviewDto>(
            reviews.Data.ToDtos(),
            reviews.TotalCount,
            reviews.PageNumber,
            reviews.PageSize);
    }

    public async Task<Pagination<ReviewDto>> GetByArtistIdAsync(int id, IPageParams pageParams)
    {
        return await GetAsync(p => reviewRepository.GetByArtistIdAsync(id, p), pageParams);
    }

    public async Task<Pagination<ReviewDto>> GetByConcertIdAsync(int id, IPageParams pageParams)
    {
        return await GetAsync(p => reviewRepository.GetByConcertIdAsync(id, p), pageParams);
    }

    public async Task<Pagination<ReviewDto>> GetByVenueIdAsync(int id, IPageParams pageParams)
    {
        return await GetAsync(p => reviewRepository.GetByVenueIdAsync(id, p), pageParams);
    }

    public async Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id)
    {
        return await reviewRepository.GetSummaryByArtistIdAsync(id);
    }

    public async Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id)
    {
        return await reviewRepository.GetSummaryByConcertIdAsync(id);
    }

    public async Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id)
    {
        return await reviewRepository.GetSummaryByVenueIdAsync(id);
    }

    public async Task<bool> CanUserReviewConcertIdAsync(int concertId)
    {
        var user = currentUser.Get();
        return await reviewRepository.CanUserIdReviewConcertIdAsync(user.Id, concertId);
    }

    public async Task<bool> CanUserReviewVenueIdAsync(int venueId)
    {
        var user = currentUser.Get();
        return await reviewRepository.CanUserIdReviewVenueIdAsync(user.Id, venueId);
    }

    public async Task<bool> CanUserReviewArtistIdAsync(int artistId)
    {
        var user = currentUser.Get();
        return await reviewRepository.CanUserIdReviewArtistIdAsync(user.Id, artistId);
    }
}
