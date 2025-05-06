using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using System.Linq.Expressions;

namespace Application.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
        Task<ReviewSummaryDto> GetSummaryByEventIdAsync(int id);
        Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);

        // Average Rating (single)
        Task<double> GetAverageRatingByArtistIdAsync(int id);
        Task<double> GetAverageRatingByEventIdAsync(int id);
        Task<double> GetAverageRatingByVenueIdAsync(int id);

        // Average Ratings (multiple)
        Task<IDictionary<int, double>> GetAverageRatingsByArtistIdsAsync(IEnumerable<int> ids);
        Task<IDictionary<int, double>> GetAverageRatingsByEventIdsAsync(IEnumerable<int> ids);
        Task<IDictionary<int, double>> GetAverageRatingsByVenueIdsAsync(IEnumerable<int> ids);

        // Paginated Review Retrieval
        Task<PaginationResponse<Review>> GetByEventIdAsync(int  eventId, PaginationParams pageParams);
        Task<PaginationResponse<Review>> GetByArtistIdAsync(int artistId, PaginationParams pageParams);
        Task<PaginationResponse<Review>> GetByVenueIdAsync(int venueId, PaginationParams pageParams);

        // Review Permission Checks
        Task<bool> CanUserIdReviewEventIdAsync(int userId, int eventId);
        Task<bool> CanUserIdReviewArtistIdAsync(int userId, int artistId);
        Task<bool> CanUserIdReviewVenueIdAsync(int userId, int venueId);

    }
}
