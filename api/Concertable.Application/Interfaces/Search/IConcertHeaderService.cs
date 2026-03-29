using Concertable.Application.DTOs;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IConcertHeaderService : IHeaderService
{
    Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams);
}
