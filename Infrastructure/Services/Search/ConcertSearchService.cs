using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Application.Mappers;

namespace Infrastructure.Services.Search
{
    public class ConcertSearchService : IConcertSearchService
    {
        private readonly IConcertSearchRepository concertSearchRepository;
        private readonly IReviewService reviewService;

        public ConcertSearchService(IConcertSearchRepository concertSearchRepository, IReviewService reviewService)
        {
            this.concertSearchRepository = concertSearchRepository;
            this.reviewService = reviewService;
        }

        public async Task<Pagination<ISearchHeader>> SearchAsync(SearchParams searchParams)
        {
            var result = await concertSearchRepository.SearchAsync(searchParams);
            var headers = result.Data.ToHeaderDtos().ToList();
            await reviewService.AddAverageRatingsAsync(headers);
            return new Pagination<ISearchHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
        }
    }
}
