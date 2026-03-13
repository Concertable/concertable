using Application.DTOs;
using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class VenueHeaderSpecification : IVenueHeaderSpecification
{
    private readonly IVenueSearchSpecification searchSpecification;
    private readonly ISearchRatingSpecification<Venue> searchRatingSpecification;

    public VenueHeaderSpecification(
        IVenueSearchSpecification searchSpecification,
        ISearchRatingSpecification<Venue> searchRatingSpecification)
    {
        this.searchSpecification = searchSpecification;
        this.searchRatingSpecification = searchRatingSpecification;
    }

    public IQueryable<VenueHeaderDto> Apply(IQueryable<Venue> query, SearchParams searchParams) =>
        searchRatingSpecification.Apply(searchSpecification.Apply(query, searchParams))
            .Select(x => new VenueHeaderDto
            {
                Id = x.Entity.Id,
                Name = x.Entity.Name,
                ImageUrl = x.Entity.ImageUrl,
                Rating = x.Rating,
                County = x.Entity.User.County ?? string.Empty,
                Town = x.Entity.User.Town ?? string.Empty,
                Latitude = x.Entity.User.Location != null ? x.Entity.User.Location.Y : 0.0,
                Longitude = x.Entity.User.Location != null ? x.Entity.User.Location.X : 0.0
            });
}
