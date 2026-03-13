using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Application.Mappers;

namespace Infrastructure.Services.Search;

public class ArtistHeaderService : IHeaderService
{
    private readonly IArtistHeaderRepository artistHeaderRepository;
    private readonly IReviewService reviewService;

    public ArtistHeaderService(IArtistHeaderRepository artistHeaderRepository, IReviewService reviewService)
    {
        this.artistHeaderRepository = artistHeaderRepository;
        this.reviewService = reviewService;
    }

    public async Task<Pagination<IHeader>> SearchAsync(SearchParams searchParams)
    {
        var result = await artistHeaderRepository.SearchAsync(searchParams);
        var headers = result.Data.ToHeaderDtos().ToList();
        await reviewService.AddAverageRatingsAsync(headers);
        return new Pagination<IHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<IEnumerable<IHeader>> GetByAmountAsync(int amount)
    {
        var headers = await artistHeaderRepository.GetByAmountAsync(amount);
        await reviewService.AddAverageRatingsAsync(headers);
        return headers;
    }

}
