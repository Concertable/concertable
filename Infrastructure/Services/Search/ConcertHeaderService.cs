using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Application.Mappers;

namespace Infrastructure.Services.Search;

public class ConcertHeaderService : IConcertHeaderService
{
    private readonly IConcertHeaderRepository concertHeaderRepository;
    private readonly IReviewService reviewService;

    public ConcertHeaderService(IConcertHeaderRepository concertHeaderRepository, IReviewService reviewService)
    {
        this.concertHeaderRepository = concertHeaderRepository;
        this.reviewService = reviewService;
    }

    public async Task<Pagination<IHeader>> SearchAsync(SearchParams searchParams)
    {
        var result = await concertHeaderRepository.SearchAsync(searchParams);
        var headers = result.Data.ToHeaderDtos().ToList();
        await reviewService.AddAverageRatingsAsync(headers);
        return new Pagination<IHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<IEnumerable<IHeader>> GetByAmountAsync(int amount)
    {
        var headers = await concertHeaderRepository.GetByAmountAsync(amount);
        await reviewService.AddAverageRatingsAsync(headers);
        return headers;
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync()
    {
        var headers = await concertHeaderRepository.GetPopularAsync();
        await reviewService.AddAverageRatingsAsync(headers);
        return headers;
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync()
    {
        var headers = await concertHeaderRepository.GetFreeAsync();
        await reviewService.AddAverageRatingsAsync(headers);
        return headers;
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams)
    {
        var headers = await concertHeaderRepository.GetRecommendedAsync(concertParams);
        await reviewService.AddAverageRatingsAsync(headers);
        return headers;
    }
}
