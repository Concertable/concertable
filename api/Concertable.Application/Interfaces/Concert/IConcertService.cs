using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;


namespace Concertable.Application.Interfaces.Concert;

public interface IConcertService
{
    Task<ConcertDetailsResponse> GetDetailsByIdAsync(int id);
    Task<ConcertDetailsResponse> GetDetailsByApplicationIdAsync(int applicationId);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id);
    Task<ConcertDto> CreateDraftAsync(int applicationId);
    Task<ConcertDto> UpdateAsync(int id, UpdateConcertRequest request);
    Task<ConcertPostResponse> PostAsync(int id, UpdateConcertRequest request);
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync();
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id);
}
