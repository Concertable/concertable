using Application.DTOs;
using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces;

public interface IConcertService
{
    Task<ConcertDto> GetDetailsByIdAsync(int id);
    Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId);
    Task<IEnumerable<ConcertDto>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetUpcomingByArtistIdAsync(int id);
    Task<ListingApplicationPurchaseResponse> BookAsync(ConcertBookingParams bookingParams);
    Task<ListingApplicationPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    Task<ConcertDto> UpdateAsync(ConcertDto concertDto);
    Task<ConcertPostResponse> PostAsync(ConcertDto concertDto);
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync();
    Task<IEnumerable<ConcertDto>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetUnpostedByVenueIdAsync(int id);
}
