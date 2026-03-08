using Application.DTOs;
using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces
{
    public interface IVenueRepository : IRepository<Venue>
    {
        Task<Venue?> GetByUserIdAsync(int id);
        Task<int?> GetIdByUserIdAsync(int userId);
        Task<IEnumerable<VenueHeaderDto>> GetHeadersByAmountAsync(int amount);
    }
}
