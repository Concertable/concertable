using Application.Interfaces;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Parameters;
using Infrastructure.Mappers;

namespace Infrastructure.Services.Search
{
    public class ArtistSearchService : IArtistSearchService
    {
        private readonly IArtistSearchRepository artistSearchRepository;
        private readonly IReviewService reviewService;

        public ArtistSearchService(IArtistSearchRepository artistSearchRepository, IReviewService reviewService)
        {
            this.artistSearchRepository = artistSearchRepository;
            this.reviewService = reviewService;
        }

        public async Task<Pagination<ISearchHeader>> SearchAsync(SearchParams searchParams)
        {
            var result = await artistSearchRepository.SearchAsync(searchParams);
            var headers = result.Data.Select(a => a.ToSearchHeader()).ToList();
            await reviewService.AddAverageRatingsAsync(headers);
            return new Pagination<ISearchHeader>(headers, result.TotalCount, result.PageNumber, result.PageSize);
        }
    }
}
