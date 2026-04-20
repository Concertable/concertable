

namespace Concertable.Search.Application.Interfaces;

public interface IArtistHeaderRepository : IHeaderRepository<ArtistHeaderDto>
{
    Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount);
}
