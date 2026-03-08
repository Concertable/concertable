using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Search
{
    public interface IVenueHeaderRepository : IHeaderRepository<Venue>
    {
        Task<IEnumerable<VenueHeaderDto>> GetByAmountAsync(int amount);
    }
}
