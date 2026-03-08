using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Application.Mappers;

namespace Infrastructure.Services.Search;

public class VenueHeaderService : IHeaderService
{
    private readonly IVenueHeaderRepository venueHeaderRepository;
    private readonly IReviewService reviewService;

    public VenueHeaderService(IVenueHeaderRepository venueHeaderRepository, IReviewService reviewService)
    {
        this.venueHeaderRepository = venueHeaderRepository;
        this.reviewService = reviewService;
    }

    public async Task<Pagination<IHeader>> SearchAsync(SearchParams searchParams)
    {
        var result = await venueHeaderRepository.SearchAsync(searchParams);
        var headers = result.Data.ToHeaderDtos().ToList();
        await reviewService.AddAverageRatingsAsync(headers);
        return new Pagination<IHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<IEnumerable<IHeader>> GetByAmountAsync(int amount)
    {
        var headers = await venueHeaderRepository.GetByAmountAsync(amount);
        await reviewService.AddAverageRatingsAsync(headers);
        return headers;
    }

    public Task<IEnumerable<IHeader>> GetPopularAsync() => Task.FromResult(Enumerable.Empty<IHeader>());

    public Task<IEnumerable<IHeader>> GetFreeAsync() => Task.FromResult(Enumerable.Empty<IHeader>());
}
