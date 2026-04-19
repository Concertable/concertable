using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using FluentResults;


namespace Concertable.Application.Interfaces.Concert;

public interface IConcertService
{
    Task<ConcertDto> GetDetailsByIdAsync(int id);
    Task<ConcertDto> GetDetailsByApplicationIdAsync(int applicationId);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUpcomingByArtistIdAsync(int id);
    Task<Result<ConcertEntity>> CreateDraftAsync(int applicationId);
    Task<ConcertUpdateResponse> UpdateAsync(int id, UpdateConcertRequest request);
    Task<ConcertPostResponse> PostAsync(int id, UpdateConcertRequest request);
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedHeadersAsync();
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetHistoryByVenueIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByArtistIdAsync(int id);
    Task<IEnumerable<ConcertSummaryDto>> GetUnpostedByVenueIdAsync(int id);
}
