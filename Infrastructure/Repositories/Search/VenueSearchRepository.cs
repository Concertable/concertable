using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
namespace Infrastructure.Repositories.Search
{
    public class VenueSearchRepository : IVenueSearchRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IVenueSearchSpecification specification;

        public VenueSearchRepository(ApplicationDbContext context, IVenueSearchSpecification specification)
        {
            this.context = context;
            this.specification = specification;
        }

        public async Task<Pagination<Venue>> SearchAsync(SearchParams searchParams)
        {
            var query = specification.Apply(context.Venues.AsQueryable(), searchParams);
            return await query.ToPaginationAsync(searchParams);
        }
    }
}
