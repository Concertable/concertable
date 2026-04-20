using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;
using LinqKit;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableConcertMappers
{
    public static IQueryable<ConcertDto> ToDto(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> concertRatings,
        IQueryable<RatingAggregate> artistRatings,
        IQueryable<RatingAggregate> venueRatings) =>
        from c in query.Where(c => c.Booking.Application.Opportunity.Venue.Location != null
                                 && c.Booking.Application.Opportunity.Venue.Address != null).AsExpandable()
        join cr in concertRatings on c.Id equals cr.EntityId into crg
        from concertRating in crg.DefaultIfEmpty()
        join ar in artistRatings on c.Booking.Application.ArtistId equals ar.EntityId into arg
        from artistRating in arg.DefaultIfEmpty()
        join vr in venueRatings on c.Booking.Application.Opportunity.VenueId equals vr.EntityId into vrg
        from venueRating in vrg.DefaultIfEmpty()
        select new ConcertDto
        {
            Id = c.Id,
            Name = c.Name,
            About = c.About,
            BannerUrl = c.BannerUrl ?? c.Booking.Application.Artist.BannerUrl,
            Avatar = c.Avatar ?? c.Booking.Application.Artist.Avatar,
            Rating = (double?)concertRating.AverageRating ?? 0.0,
            Price = c.Price,
            TotalTickets = c.TotalTickets,
            AvailableTickets = c.AvailableTickets,
            DatePosted = c.DatePosted,
            StartDate = c.Booking.Application.Opportunity.Period.Start,
            EndDate = c.Booking.Application.Opportunity.Period.End,
            Genres = GenreSelectors.FromConcert.Invoke(c),
            Venue = new ConcertVenueDto
            {
                Id = c.Booking.Application.Opportunity.Venue.Id,
                Name = c.Booking.Application.Opportunity.Venue.Name,
                Rating = (double?)venueRating.AverageRating ?? 0.0,
                County = c.Booking.Application.Opportunity.Venue.Address!.County,
                Town = c.Booking.Application.Opportunity.Venue.Address!.Town,
                Latitude = c.Booking.Application.Opportunity.Venue.Location!.Y,
                Longitude = c.Booking.Application.Opportunity.Venue.Location!.X
            },
            Artist = new ConcertArtistDto
            {
                Id = c.Booking.Application.Artist.Id,
                Name = c.Booking.Application.Artist.Name,
                Avatar = c.Booking.Application.Artist.Avatar,
                County = c.Booking.Application.Artist.Address.County ?? string.Empty,
                Town = c.Booking.Application.Artist.Address.Town ?? string.Empty,
                Rating = (double?)artistRating.AverageRating ?? 0.0,
                Genres = GenreSelectors.FromArtist.Invoke(c.Booking.Application.Artist)
            }
        };

    public static IQueryable<ConcertSummaryDto> ToSummaryDto(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> artistRatings,
        IQueryable<RatingAggregate> venueRatings) =>
        from c in query.Where(c => c.Booking.Application.Opportunity.Venue.Location != null
                                 && c.Booking.Application.Opportunity.Venue.Address != null).AsExpandable()
        join ar in artistRatings on c.Booking.Application.ArtistId equals ar.EntityId into arg
        from artistRating in arg.DefaultIfEmpty()
        join vr in venueRatings on c.Booking.Application.Opportunity.VenueId equals vr.EntityId into vrg
        from venueRating in vrg.DefaultIfEmpty()
        select new ConcertSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Avatar ?? c.Booking.Application.Artist.Avatar,
            Price = c.Price,
            TotalTickets = c.TotalTickets,
            AvailableTickets = c.AvailableTickets,
            DatePosted = c.DatePosted,
            StartDate = c.Booking.Application.Opportunity.Period.Start,
            EndDate = c.Booking.Application.Opportunity.Period.End,
            Venue = new ConcertVenueSummaryDto
            {
                Id = c.Booking.Application.Opportunity.Venue.Id,
                Name = c.Booking.Application.Opportunity.Venue.Name,
                Rating = (double?)venueRating.AverageRating ?? 0.0
            },
            Artist = new ConcertArtistSummaryDto
            {
                Id = c.Booking.Application.Artist.Id,
                Name = c.Booking.Application.Artist.Name,
                Rating = (double?)artistRating.AverageRating ?? 0.0,
                Genres = GenreSelectors.FromArtist.Invoke(c.Booking.Application.Artist)
            }
        };

}
