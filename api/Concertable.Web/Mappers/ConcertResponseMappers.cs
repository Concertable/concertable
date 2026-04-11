using Concertable.Application.DTOs;
using Concertable.Web.Responses;

namespace Concertable.Web.Mappers;

public static class ConcertResponseMappers
{
    public static ConcertDetailsResponse ToDetailsResponse(this ConcertDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl ?? dto.Artist.BannerUrl ?? string.Empty,
        Avatar = dto.Avatar ?? dto.Artist.Avatar ?? string.Empty,
        Rating = dto.Rating,
        Price = dto.Price,
        TotalTickets = dto.TotalTickets,
        AvailableTickets = dto.AvailableTickets,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        DatePosted = dto.DatePosted,
        Genres = dto.Genres,
        Artist = new ConcertArtistResponse
        {
            Id = dto.Artist.Id,
            Name = dto.Artist.Name,
            About = dto.Artist.About,
            Rating = dto.Artist.Rating,
            County = dto.Artist.County,
            Town = dto.Artist.Town,
            Genres = dto.Artist.Genres
        },
        Venue = new ConcertVenueResponse
        {
            Id = dto.Venue.Id,
            Name = dto.Venue.Name,
            County = dto.Venue.County,
            Town = dto.Venue.Town,
            Latitude = dto.Venue.Latitude,
            Longitude = dto.Venue.Longitude
        }
    };
}
