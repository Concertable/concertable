using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Infrastructure.Mappers;

namespace Infrastructure.Services.Search
{
    public class VenueSearchService : IVenueSearchService
    {
        private readonly IVenueSearchRepository venueSearchRepository;
        private readonly IReviewService reviewService;

        public VenueSearchService(IVenueSearchRepository venueSearchRepository, IReviewService reviewService)
        {
            this.venueSearchRepository = venueSearchRepository;
            this.reviewService = reviewService;
        }

        public async Task<Pagination<ISearchHeader>> SearchAsync(SearchParams searchParams)
        {
            var result = await venueSearchRepository.SearchAsync(searchParams);
            var headers = result.Data.Select(v => v.ToSearchHeader()).ToList();
            await reviewService.AddAverageRatingsAsync(headers);
            return new Pagination<ISearchHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
        }
    }
}
