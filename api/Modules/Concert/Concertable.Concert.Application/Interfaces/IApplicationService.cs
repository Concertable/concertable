using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IApplicationService
{
    Task<ApplicationDto> GetByIdAsync(int id);
    Task<IEnumerable<ApplicationDto>> GetByOpportunityIdAsync(int id);
    Task<IEnumerable<ApplicationDto>> GetPendingForArtistAsync();
    Task<IEnumerable<ApplicationDto>> GetRecentDeniedForArtistAsync();
    Task<ApplicationDto> ApplyAsync(int opportunityId);
    Task<AcceptCheckout?> CheckoutAsync(int applicationId);
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null);
    Task<(ArtistReadModel, VenueReadModel)?> GetArtistAndVenueByIdAsync(int id);
}
