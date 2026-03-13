using Application.DTOs;
using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IConcertHeaderRepository : IHeaderRepository<Concert>
{
    Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount);
    Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams);
}
