using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableConcertMappers
{
    public static IQueryable<ConcertDto> ToDto(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> concertRatings,
        IQueryable<RatingAggregate> artistRatings) =>
        from c in query
        join cr in concertRatings on c.Id equals cr.EntityId into crg
        from concertRating in crg.DefaultIfEmpty()
        join ar in artistRatings on c.Application.ArtistId equals ar.EntityId into arg
        from artistRating in arg.DefaultIfEmpty()
        select new ConcertDto
        {
            Id = c.Id,
            Name = c.Name,
            About = c.About,
            BannerUrl = c.BannerUrl,
            Avatar = c.Avatar,
            Rating = (double?)concertRating.AverageRating ?? 0.0,
            Price = c.Price,
            TotalTickets = c.TotalTickets,
            AvailableTickets = c.AvailableTickets,
            DatePosted = c.DatePosted,
            StartDate = c.Application.Opportunity.StartDate,
            EndDate = c.Application.Opportunity.EndDate,
            Genres = c.ConcertGenres.Select(cg => new GenreDto(cg.Genre.Id, cg.Genre.Name)),
            Venue = new ConcertVenueDto
            {
                Id = c.Application.Opportunity.Venue.Id,
                Name = c.Application.Opportunity.Venue.Name,
                County = c.Application.Opportunity.Venue.User.County ?? string.Empty,
                Town = c.Application.Opportunity.Venue.User.Town ?? string.Empty,
                Latitude = c.Application.Opportunity.Venue.User.Location!.Y,
                Longitude = c.Application.Opportunity.Venue.User.Location!.X
            },
            Artist = new ConcertArtistDto
            {
                Id = c.Application.Artist.Id,
                Name = c.Application.Artist.Name,
                About = c.Application.Artist.About,
                Avatar = c.Application.Artist.User.Avatar,
                BannerUrl = c.Application.Artist.BannerUrl,
                County = c.Application.Artist.User.County ?? string.Empty,
                Town = c.Application.Artist.User.Town ?? string.Empty,
                Rating = (double?)artistRating.AverageRating ?? 0.0,
                Genres = c.Application.Artist.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name))
            }
        };

    public static IQueryable<ConcertHeaderDto> ToHeaderDtos(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from c in query
        join r in ratings on c.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ConcertHeaderDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Application.Artist.User.Avatar,
            Rating = rating.AverageRating,
            StartDate = c.Application.Opportunity.StartDate,
            EndDate = c.Application.Opportunity.EndDate,
            DatePosted = c.DatePosted,
            County = c.Application.Opportunity.Venue.User.County ?? string.Empty,
            Town = c.Application.Opportunity.Venue.User.Town ?? string.Empty
        };
}
