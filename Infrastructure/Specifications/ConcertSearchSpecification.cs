using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Specifications
{
    public class ConcertSearchSpecification : IConcertSearchSpecification
    {
        private readonly ISearchSpecification<Concert> searchSpecification;

        public ConcertSearchSpecification(ISearchSpecification<Concert> searchSpecification)
        {
            this.searchSpecification = searchSpecification;
        }

        public IQueryable<Concert> Apply(IQueryable<Concert> query, SearchParams searchParams)
        {
            query = query
                .Include(e => e.Application).ThenInclude(a => a.Artist)
                .Include(e => e.Application).ThenInclude(a => a.Listing).ThenInclude(l => l.Venue).ThenInclude(v => v.User)
                .Where(e => e.DatePosted != null)
                .Where(e => e.Application.Listing.EndDate > DateTime.UtcNow);

            if (searchParams.Date != null)
                query = query.Where(e => e.Application.Listing.StartDate >= searchParams.Date);

            if (searchParams.GenreIds?.Any() == true)
                query = query.Where(e => e.ConcertGenres.Any(eg => searchParams.GenreIds.Contains(eg.GenreId)));

            if (searchParams.ShowHistory != true)
                query = query.Where(e => e.Application.Listing.StartDate >= DateTime.UtcNow);

            if (searchParams.ShowSold != true)
                query = query.Where(e => e.AvailableTickets > 0);

            query = searchSpecification.Apply(query, searchParams);

            return searchParams.Sort?.ToLower() switch
            {
                "name_asc" => query.OrderBy(e => e.Name),
                "name_desc" => query.OrderByDescending(e => e.Name),
                "date_asc" => query.OrderBy(e => e.Application.Listing.StartDate),
                "date_desc" => query.OrderByDescending(e => e.Application.Listing.StartDate),
                _ => query.OrderBy(e => e.Application.Listing.StartDate)
            };
        }
    }
}
