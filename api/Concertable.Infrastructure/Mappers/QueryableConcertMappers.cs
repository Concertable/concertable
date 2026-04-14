using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableConcertMappers
{
    public static IQueryable<ConcertDto> ToDto(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> concertRatings,
        IQueryable<RatingAggregate> artistRatings,
        IQueryable<RatingAggregate> venueRatings) =>
        from c in query.Where(c => c.Application.Opportunity.Venue.User.Location != null
                                 && c.Application.Opportunity.Venue.User.Address != null)
        join cr in concertRatings on c.Id equals cr.EntityId into crg
        from concertRating in crg.DefaultIfEmpty()
        join ar in artistRatings on c.Application.ArtistId equals ar.EntityId into arg
        from artistRating in arg.DefaultIfEmpty()
        join vr in venueRatings on c.Application.Opportunity.VenueId equals vr.EntityId into vrg
        from venueRating in vrg.DefaultIfEmpty()
        select new ConcertDto
        {
            Id = c.Id,
            Name = c.Name,
            About = c.About,
            BannerUrl = c.BannerUrl ?? c.Application.Artist.BannerUrl,
            Avatar = c.Avatar ?? c.Application.Artist.User.Avatar,
            Rating = (double?)concertRating.AverageRating ?? 0.0,
            Price = c.Price,
            TotalTickets = c.TotalTickets,
            AvailableTickets = c.AvailableTickets,
            DatePosted = c.DatePosted,
            StartDate = c.Application.Opportunity.Period.Start,
            EndDate = c.Application.Opportunity.Period.End,
            Genres = c.ConcertGenres.Select(cg => new GenreDto(cg.Genre.Id, cg.Genre.Name)),
            Venue = new ConcertVenueDto
            {
                Id = c.Application.Opportunity.Venue.Id,
                Name = c.Application.Opportunity.Venue.Name,
                Rating = (double?)venueRating.AverageRating ?? 0.0,
                County = c.Application.Opportunity.Venue.User.Address!.County,
                Town = c.Application.Opportunity.Venue.User.Address!.Town,
                Latitude = c.Application.Opportunity.Venue.User.Location!.Y,
                Longitude = c.Application.Opportunity.Venue.User.Location!.X
            },
            Artist = new ConcertArtistDto
            {
                Id = c.Application.Artist.Id,
                Name = c.Application.Artist.Name,
                Avatar = c.Application.Artist.User.Avatar,
                County = c.Application.Artist.User.Address.County ?? string.Empty,
                Town = c.Application.Artist.User.Address.Town ?? string.Empty,
                Rating = (double?)artistRating.AverageRating ?? 0.0,
                Genres = c.Application.Artist.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name))
            }
        };

    public static IQueryable<ConcertSummaryDto> ToSummaryDto(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> artistRatings,
        IQueryable<RatingAggregate> venueRatings) =>
        from c in query.Where(c => c.Application.Opportunity.Venue.User.Location != null
                                 && c.Application.Opportunity.Venue.User.Address != null)
        join ar in artistRatings on c.Application.ArtistId equals ar.EntityId into arg
        from artistRating in arg.DefaultIfEmpty()
        join vr in venueRatings on c.Application.Opportunity.VenueId equals vr.EntityId into vrg
        from venueRating in vrg.DefaultIfEmpty()
        select new ConcertSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Avatar ?? c.Application.Artist.User.Avatar,
            Price = c.Price,
            TotalTickets = c.TotalTickets,
            AvailableTickets = c.AvailableTickets,
            DatePosted = c.DatePosted,
            StartDate = c.Application.Opportunity.Period.Start,
            EndDate = c.Application.Opportunity.Period.End,
            Venue = new ConcertVenueSummaryDto
            {
                Id = c.Application.Opportunity.Venue.Id,
                Name = c.Application.Opportunity.Venue.Name,
                Rating = (double?)venueRating.AverageRating ?? 0.0
            },
            Artist = new ConcertArtistSummaryDto
            {
                Id = c.Application.Artist.Id,
                Name = c.Application.Artist.Name,
                Rating = (double?)artistRating.AverageRating ?? 0.0,
                Genres = c.Application.Artist.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name))
            }
        };

    public static IQueryable<ConcertHeaderDto> ToHeaderDtos(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from c in query.Where(c => c.Application.Opportunity.Venue.User.Location != null
                                 && c.Application.Opportunity.Venue.User.Address != null)
        join r in ratings on c.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ConcertHeaderDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Application.Artist.User.Avatar,
            Rating = rating.AverageRating,
            StartDate = c.Application.Opportunity.Period.Start,
            EndDate = c.Application.Opportunity.Period.End,
            DatePosted = c.DatePosted,
            County = c.Application.Opportunity.Venue.User.Address.County ?? string.Empty,
            Town = c.Application.Opportunity.Venue.User.Address.Town ?? string.Empty
        };
}
