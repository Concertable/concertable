using Core.Interfaces;
using Application.DTOs;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Application.Requests;

namespace Application.Interfaces;

public interface IReviewService
{
    Task AddAverageRatingsAsync(IEnumerable<ArtistHeaderDto> headers);
    Task AddAverageRatingsAsync(IEnumerable<VenueHeaderDto> headers);
    Task AddAverageRatingsAsync(IEnumerable<ConcertHeaderDto> headers);

    Task SetAverageRatingAsync(VenueDto venue);

    Task<Pagination<ReviewDto>> GetByVenueIdAsync(int id, IPageParams pageParams);
    Task<Pagination<ReviewDto>> GetByArtistIdAsync(int id, IPageParams pageParams);
    Task<Pagination<ReviewDto>> GetByConcertIdAsync(int id, IPageParams pageParams);

    Task<ReviewSummaryDto> GetSummaryByVenueIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByArtistIdAsync(int id);
    Task<ReviewSummaryDto> GetSummaryByConcertIdAsync(int id);

    Task<ReviewDto> CreateAsync(CreateReviewRequest review);
    Task<bool> CanUserReviewConcertIdAsync(int concertId);
    Task<bool> CanUserReviewVenueIdAsync(int venueId);
    Task<bool> CanUserReviewArtistIdAsync(int artistId);
}
