using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Infrastructure.Mappers;

namespace Infrastructure.Services.Search
{
    public class EventSearchService : IEventSearchService
    {
        private readonly IEventSearchRepository eventSearchRepository;
        private readonly IReviewService reviewService;

        public EventSearchService(IEventSearchRepository eventSearchRepository, IReviewService reviewService)
        {
            this.eventSearchRepository = eventSearchRepository;
            this.reviewService = reviewService;
        }

        public async Task<Pagination<ISearchHeader>> SearchAsync(SearchParams searchParams)
        {
            var result = await eventSearchRepository.SearchAsync(searchParams);
            var headers = result.Data.Select(e => e.ToSearchHeader()).ToList();
            await reviewService.AddAverageRatingsAsync(headers);
            return new Pagination<ISearchHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
        }
    }
}
