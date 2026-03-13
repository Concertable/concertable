using Application.DTOs;
using Application.Interfaces.Search;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search;

public class ConcertHeaderRepository : IConcertHeaderRepository
{
    private readonly ApplicationDbContext context;
    private readonly IConcertHeaderSpecification specification;
    private readonly IRatingSpecification<Concert> ratingSpecification;
    private readonly IGeometrySpecification<Concert> geometrySpecification;
    private readonly TimeProvider timeProvider;

    public ConcertHeaderRepository(
        ApplicationDbContext context,
        IConcertHeaderSpecification specification,
        IRatingSpecification<Concert> ratingSpecification,
        IGeometrySpecification<Concert> geometrySpecification,
        TimeProvider timeProvider)
    {
        this.context = context;
        this.specification = specification;
        this.ratingSpecification = ratingSpecification;
        this.geometrySpecification = geometrySpecification;
        this.timeProvider = timeProvider;
    }

    public async Task<Pagination<ConcertHeaderDto>> SearchAsync(SearchParams searchParams)
    {
        var query = specification.Apply(context.Concerts.AsQueryable(), searchParams);
        return await query.ToPaginationAsync(searchParams);
    }

    private IQueryable<Concert> ActiveConcerts() =>
        context.Concerts.Where(c => c.DatePosted != null && c.Application.Opportunity.EndDate > timeProvider.GetUtcNow());

    private IQueryable<ConcertHeaderDto> ToHeaderDtos(IQueryable<Concert> query)
    {
        var ratings = ratingSpecification.Apply(context.Reviews);

        return from c in query
               join r in ratings on c.Id equals r.EntityId into rg
               from rating in rg.DefaultIfEmpty()
               select new ConcertHeaderDto
               {
                   Id = c.Id,
                   Name = c.Name,
                   ImageUrl = c.Application.Artist.ImageUrl,
                   Rating = rating != null ? rating.AverageRating : 0.0,
                   StartDate = c.Application.Opportunity.StartDate,
                   EndDate = c.Application.Opportunity.EndDate,
                   DatePosted = c.DatePosted,
                   County = c.Application.Opportunity.Venue.User.County ?? string.Empty,
                   Town = c.Application.Opportunity.Venue.User.Town ?? string.Empty,
                   Latitude = c.Application.Opportunity.Venue.User.Location != null ? c.Application.Opportunity.Venue.User.Location.Y : 0.0,
                   Longitude = c.Application.Opportunity.Venue.User.Location != null ? c.Application.Opportunity.Venue.User.Location.X : 0.0
               };
    }

    public async Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount) =>
        await ToHeaderDtos(ActiveConcerts().OrderByDescending(c => c.DatePosted))
            .Take(amount)
            .ToListAsync();

    public async Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync() =>
        await ToHeaderDtos(ActiveConcerts().OrderByDescending(c => c.TotalTickets - c.AvailableTickets))
            .Take(10)
            .ToListAsync();

    public async Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync() =>
        await ToHeaderDtos(ActiveConcerts().Where(c => c.Price == 0).OrderByDescending(c => c.DatePosted))
            .Take(10)
            .ToListAsync();

    public async Task<IEnumerable<ConcertHeaderDto>> GetRecommendedAsync(ConcertParams concertParams)
    {
        var query = ActiveConcerts();

        if (concertParams.GenreIds.Any())
            query = query.Where(c => c.ConcertGenres.Any(eg => concertParams.GenreIds.Contains(eg.GenreId)));

        query = geometrySpecification.Apply(query, concertParams);

        query = concertParams.OrderByRecent
            ? query.OrderByDescending(c => c.DatePosted)
            : query.OrderBy(c => c.Application.Opportunity.StartDate);

        return await ToHeaderDtos(query).Take(10).ToListAsync();
    }
}
