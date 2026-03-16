using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class ConcertSearchSpecification : IConcertSearchSpecification
{
    private readonly ISearchSpecification<ConcertEntity> searchSpecification;
    private readonly TimeProvider timeProvider;

    public ConcertSearchSpecification(
        ISearchSpecification<ConcertEntity> searchSpecification,
        TimeProvider timeProvider)
    {
        this.searchSpecification = searchSpecification;
        this.timeProvider = timeProvider;
    }

    public IQueryable<ConcertEntity> Apply(IQueryable<ConcertEntity> query, SearchParams searchParams)
    {
        query = query
            .Where(e => e.DatePosted != null)
            .Where(e => e.Application.Opportunity.EndDate > timeProvider.GetUtcNow());

        if (searchParams.Date != null)
            query = query.Where(e => e.Application.Opportunity.StartDate >= searchParams.Date);

        if (searchParams.GenreIds?.Any() == true)
            query = query.Where(e => e.ConcertGenres.Any(eg => searchParams.GenreIds.Contains(eg.GenreId)));

        if (searchParams.ShowHistory == false)
            query = query.Where(e => e.Application.Opportunity.StartDate >= timeProvider.GetUtcNow());

        if (searchParams.ShowSold == false)
            query = query.Where(e => e.AvailableTickets > 0);

        query = searchSpecification.Apply(query, searchParams);

        return searchParams.Sort?.ToLower() switch
        {
            "date_asc" => query.OrderBy(e => e.Application.Opportunity.StartDate),
            "date_desc" => query.OrderByDescending(e => e.Application.Opportunity.StartDate),
            _ => query.OrderBy(e => e.Application.Opportunity.StartDate)
        };
    }
}
