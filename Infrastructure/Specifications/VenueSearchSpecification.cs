using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Specifications
{
    public class VenueSearchSpecification : IVenueSearchSpecification
    {
        private readonly ISearchSpecification<Venue> searchSpecification;

        public VenueSearchSpecification(ISearchSpecification<Venue> searchSpecification)
        {
            this.searchSpecification = searchSpecification;
        }

        public IQueryable<Venue> Apply(IQueryable<Venue> query, SearchParams searchParams)
        {
            query = query.Include(v => v.User);

            query = searchSpecification.Apply(query, searchParams);

            return searchParams.Sort?.ToLower() switch
            {
                "name_asc" => query.OrderBy(v => v.Name),
                "name_desc" => query.OrderByDescending(v => v.Name),
                _ => query.OrderBy(v => v.Id)
            };
        }
    }
}
