using Concertable.Application.DTOs;
using Concertable.Application.Results;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IArtistHeaderRepository
{
    Task<IPagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount);
}
