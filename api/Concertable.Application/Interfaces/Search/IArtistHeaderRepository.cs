using Application.DTOs;
using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IArtistHeaderRepository
{
    Task<Pagination<ArtistHeaderDto>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<ArtistHeaderDto>> GetByAmountAsync(int amount);
}
