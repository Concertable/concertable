using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityApplicationService
{
    Task<OpportunityApplicationDto> GetByIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationDto>> GetByOpportunityIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationDto>> GetPendingForArtistAsync();
    Task<IEnumerable<OpportunityApplicationDto>> GetRecentDeniedForArtistAsync();
    Task<OpportunityApplicationDto> ApplyAsync(int opportunityId);
    Task AcceptAsync(int applicationId, string? paymentMethodId = null);
    Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
}
