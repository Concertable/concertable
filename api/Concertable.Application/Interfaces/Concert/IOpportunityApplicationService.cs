using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Artist.Application.DTOs;
using Concertable.Venue.Application.DTOs;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityApplicationService
{
    Task<OpportunityApplicationDto> GetByIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationDto>> GetByOpportunityIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationDto>> GetPendingForArtistAsync();
    Task<IEnumerable<OpportunityApplicationDto>> GetRecentDeniedForArtistAsync();
    Task<OpportunityApplicationDto> ApplyAsync(int opportunityId);
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null);
    Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
}
