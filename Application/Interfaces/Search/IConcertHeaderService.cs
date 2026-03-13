using Application.DTOs;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IConcertHeaderService : IHeaderService
{
    Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams);
}
