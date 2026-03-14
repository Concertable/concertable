using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;

namespace Infrastructure.Services.Search;

public class ArtistHeaderService : IHeaderService
{
    private readonly IArtistHeaderRepository artistHeaderRepository;

    public ArtistHeaderService(IArtistHeaderRepository artistHeaderRepository)
    {
        this.artistHeaderRepository = artistHeaderRepository;
    }

    public async Task<Pagination<IHeader>> SearchAsync(SearchParams searchParams)
    {
        var result = await artistHeaderRepository.SearchAsync(searchParams);
        return new Pagination<IHeader>(result.Data, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<IEnumerable<IHeader>> GetByAmountAsync(int amount) =>
        await artistHeaderRepository.GetByAmountAsync(amount);
}
