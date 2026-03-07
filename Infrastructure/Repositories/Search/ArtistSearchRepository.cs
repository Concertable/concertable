using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
namespace Infrastructure.Repositories.Search
{
    public class ArtistSearchRepository : IArtistSearchRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IArtistSearchSpecification specification;

        public ArtistSearchRepository(ApplicationDbContext context, IArtistSearchSpecification specification)
        {
            this.context = context;
            this.specification = specification;
        }

        public async Task<Pagination<Artist>> SearchAsync(SearchParams searchParams)
        {
            var query = specification.Apply(context.Artists.AsQueryable(), searchParams);
            return await query.ToPaginationAsync(searchParams);
        }
    }
}
