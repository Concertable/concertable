using Application.DTOs;
using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IVenueHeaderRepository
{
    Task<Pagination<VenueHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount);
}
