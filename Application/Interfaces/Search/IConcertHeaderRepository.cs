using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Search
{
    public interface IConcertHeaderRepository : IHeaderRepository<Concert>
    {
        Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount);
        Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
        Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    }
}
