using Application.DTOs;

namespace Application.Interfaces.Concert;

public interface IConcertApplicationService
{
    Task<ConcertApplicationDto> GetByIdAsync(int id);
    Task<IEnumerable<ConcertApplicationDto>> GetByOpportunityIdAsync(int id);
    Task<IEnumerable<ConcertApplicationDto>> GetPendingForArtistAsync();
    Task<IEnumerable<ConcertApplicationDto>> GetRecentDeniedForArtistAsync();
    Task ApplyAsync(int opportunityId);
    Task AcceptAsync(int applicationId);
    Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
}
