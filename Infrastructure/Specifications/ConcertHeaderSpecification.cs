using Application.DTOs;
using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class ConcertHeaderSpecification : IConcertHeaderSpecification
{
    private readonly IConcertSearchSpecification searchSpecification;
    private readonly ISearchRatingSpecification<Concert> searchRatingSpecification;

    public ConcertHeaderSpecification(
        IConcertSearchSpecification searchSpecification,
        ISearchRatingSpecification<Concert> searchRatingSpecification)
    {
        this.searchSpecification = searchSpecification;
        this.searchRatingSpecification = searchRatingSpecification;
    }

    public IQueryable<ConcertHeaderDto> Apply(IQueryable<Concert> query, SearchParams searchParams) =>
        searchRatingSpecification.Apply(searchSpecification.Apply(query, searchParams))
            .Select(x => new ConcertHeaderDto
            {
                Id = x.Entity.Id,
                Name = x.Entity.Name,
                ImageUrl = x.Entity.Application.Artist.ImageUrl,
                Rating = x.Rating,
                StartDate = x.Entity.Application.Opportunity.StartDate,
                EndDate = x.Entity.Application.Opportunity.EndDate,
                DatePosted = x.Entity.DatePosted,
                County = x.Entity.Application.Opportunity.Venue.User.County ?? string.Empty,
                Town = x.Entity.Application.Opportunity.Venue.User.Town ?? string.Empty,
                Latitude = x.Entity.Application.Opportunity.Venue.User.Location != null ? x.Entity.Application.Opportunity.Venue.User.Location.Y : 0.0,
                Longitude = x.Entity.Application.Opportunity.Venue.User.Location != null ? x.Entity.Application.Opportunity.Venue.User.Location.X : 0.0
            });
}
