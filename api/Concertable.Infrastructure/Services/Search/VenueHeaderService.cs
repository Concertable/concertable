using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Search;
using Concertable.Application.Interfaces;
using Concertable.Application.Results;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Services.Search;

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
