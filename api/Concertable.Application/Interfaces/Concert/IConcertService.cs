using Application.DTOs;
using Application.Requests;
using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Concert;

public interface IConcertService
{
    Task<ConcertDto> GetDetailsByIdAsync(int id);
    Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId);
    Task<IEnumerable<ConcertDto>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetUpcomingByArtistIdAsync(int id);
    Task<ConcertApplicationPurchaseResponse> BookAsync(ConcertBookingParams bookingParams);
    Task<ConcertApplicationPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    Task<ConcertDto> UpdateAsync(int id, UpdateConcertRequest request);
    Task<ConcertPostResponse> PostAsync(int id, UpdateConcertRequest request);
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync();
    Task<IEnumerable<ConcertDto>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<ConcertDto>> GetUnpostedByVenueIdAsync(int id);
}
