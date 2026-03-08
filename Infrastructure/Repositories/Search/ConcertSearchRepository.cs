using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
namespace Infrastructure.Repositories.Search
{
    public class ConcertSearchRepository : IConcertSearchRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IConcertSearchSpecification specification;

        public ConcertSearchRepository(ApplicationDbContext context, IConcertSearchSpecification specification)
        {
            this.context = context;
            this.specification = specification;
        }

        public async Task<Pagination<Concert>> SearchAsync(SearchParams searchParams)
        {
            var query = specification.Apply(context.Concerts.AsQueryable(), searchParams);
            return await query.ToPaginationAsync(searchParams);
        }
    }
}
