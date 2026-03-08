using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Search;

public interface IArtistHeaderRepository : IHeaderRepository<Artist>
{
    Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount);
}
