using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
namespace Infrastructure.Repositories.Search
{
    public class EventSearchRepository : IEventSearchRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IEventSearchSpecification specification;

        public EventSearchRepository(ApplicationDbContext context, IEventSearchSpecification specification)
        {
            this.context = context;
            this.specification = specification;
        }

        public async Task<Pagination<Event>> SearchAsync(SearchParams searchParams)
        {
            var query = specification.Apply(context.Events.AsQueryable(), searchParams);
            return await query.ToPaginationAsync(searchParams);
        }
    }
}
