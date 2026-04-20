using Concertable.Core.Parameters;

namespace Concertable.Search.Contracts;

public interface IConcertHeaderModule : IHeaderModule
{
    Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync();
    Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams);
}
