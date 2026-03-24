using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;

namespace Infrastructure.Services.Search;

public class VenueHeaderService : IHeaderService
{
    private readonly IVenueHeaderRepository venueHeaderRepository;

    public VenueHeaderService(IVenueHeaderRepository venueHeaderRepository)
    {
        this.venueHeaderRepository = venueHeaderRepository;
    }

    public async Task<IPagination<IHeader>> SearchAsync(SearchParams searchParams)
    {
        var result = await venueHeaderRepository.SearchAsync(searchParams);
        return new Pagination<VenueHeaderDto>(result.Data, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<IEnumerable<IHeader>> GetByAmountAsync(int amount) =>
        await venueHeaderRepository.GetByAmountAsync(amount);
}
