using Application.DTOs;
using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IConcertHeaderRepository
{
    Task<Pagination<ConcertHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount);
    Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams);
}
