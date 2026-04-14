using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IConcertHeaderRepository
{
    Task<IPagination<ConcertHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount);
    Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams);
}
