using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IOpportunityApplicationService
{
    Task<OpportunityApplicationDto> GetByIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationDto>> GetByOpportunityIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationDto>> GetPendingForArtistAsync();
    Task<IEnumerable<OpportunityApplicationDto>> GetRecentDeniedForArtistAsync();
    Task<OpportunityApplicationDto> ApplyAsync(int opportunityId);
    Task<AcceptCheckout> CheckoutAsync(int applicationId);
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null);
    Task<(ArtistReadModel, VenueReadModel)?> GetArtistAndVenueByIdAsync(int id);
}
