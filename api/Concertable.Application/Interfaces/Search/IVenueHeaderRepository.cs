using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IVenueHeaderRepository
{
    Task<Pagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount);
}
