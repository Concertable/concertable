using Application.DTOs;
using Application.Interfaces.Search;
using Core.Entities;
using Core.Parameters;

namespace Infrastructure.Specifications;

public class ArtistHeaderSpecification : IArtistHeaderSpecification
{
    private readonly IArtistSearchSpecification searchSpecification;
    private readonly ISearchRatingSpecification<Artist> searchRatingSpecification;

    public ArtistHeaderSpecification(
        IArtistSearchSpecification searchSpecification,
        ISearchRatingSpecification<Artist> searchRatingSpecification)
    {
        this.searchSpecification = searchSpecification;
        this.searchRatingSpecification = searchRatingSpecification;
    }

    public IQueryable<ArtistHeaderDto> Apply(IQueryable<Artist> query, SearchParams searchParams) =>
        searchRatingSpecification.Apply(searchSpecification.Apply(query, searchParams))
            .Select(x => new ArtistHeaderDto
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
