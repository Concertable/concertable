using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReviewService
    {
        Task AddAverageRatingsAsync(IEnumerable<ArtistHeaderDto> headers);
        Task AddAverageRatingsAsync(IEnumerable<VenueHeaderDto> headers);
        Task AddAverageRatingsAsync(IEnumerable<EventHeaderDto> headers);

        Task<PaginationResponse<ReviewDto>> GetByVenueIdAsync(int id, PaginationParams pageParams);
        Task<PaginationResponse<ReviewDto>> GetByArtistIdAsync(int id, PaginationParams pageParams);
        Task<PaginationResponse<ReviewDto>> GetByEventIdAsync(int id, PaginationParams pageParams);

        Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);
        Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
        Task<ReviewSummaryDto> GetSummaryByEventIdAsync(int id);

        Task<ReviewDto> CreateAsync(ReviewDto review);
        Task<bool> CanUserReviewEventIdAsync(int eventId);
        Task<bool> CanUserReviewVenueIdAsync(int venueId);
        Task<bool> CanUserReviewArtistIdAsync(int artistId);
    }
}
