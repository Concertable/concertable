using Concertable.Application.DTOs;
using Concertable.Application.Results;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IVenueHeaderRepository
{
    Task<IPagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount);
}
