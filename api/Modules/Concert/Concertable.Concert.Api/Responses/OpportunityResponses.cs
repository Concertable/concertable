using Concertable.Contract.Contracts;

namespace Concertable.Concert.Api.Responses;

internal record OpportunityResponse(
    int Id,
    int VenueId,
    int ContractId,
    IContract Contract,
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<GenreDto> Genres,
    OpportunityActions Actions);

internal record OpportunityActions(ActionLink Apply);
