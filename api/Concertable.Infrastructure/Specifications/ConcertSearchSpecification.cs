using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Infrastructure.Specifications;

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
            .Where(e => e.Application.Opportunity.Period.End > timeProvider.GetUtcNow());

        if (searchParams.From != null)
            query = query.Where(e => DateOnly.FromDateTime(e.Application.Opportunity.Period.Start) >= searchParams.From);

        if (searchParams.GenreIds?.Any() == true)
            query = query.Where(e => e.ConcertGenres.Any(eg => searchParams.GenreIds.Contains(eg.GenreId)));

        if (searchParams.ShowHistory == false)
            query = query.Where(e => e.Application.Opportunity.Period.Start >= timeProvider.GetUtcNow());

        if (searchParams.ShowSold == false)
            query = query.Where(e => e.AvailableTickets > 0);

        return searchSpecification.Apply(query, searchParams.SearchTerm);
    }
}
