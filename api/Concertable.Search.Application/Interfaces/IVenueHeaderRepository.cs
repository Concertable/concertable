

namespace Concertable.Search.Application.Interfaces;

public interface IVenueHeaderRepository : IHeaderRepository<VenueHeaderDto>
{
    Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount);
}
