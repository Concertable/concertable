using Application.DTOs;
using Core.Entities;

namespace Infrastructure.Mappers
{
    public static class EventMappers
    {
        public static EventHeaderDto ToSearchHeader(this Event @event) => new EventHeaderDto
        {
            Id = @event.Id,
            Name = @event.Name,
            ImageUrl = @event.Application?.Artist?.ImageUrl,
            StartDate = @event.Application?.Listing?.StartDate ?? default,
            EndDate = @event.Application?.Listing?.EndDate ?? default,
            County = @event.Application?.Listing?.Venue?.User?.County,
            Town = @event.Application?.Listing?.Venue?.User?.Town,
            Latitude = @event.Application?.Listing?.Venue?.User?.Location?.Y,
            Longitude = @event.Application?.Listing?.Venue?.User?.Location?.X,
            DatePosted = @event.DatePosted
        };

        /// <summary>
        /// Requires Application.Artist, Application.Listing.Venue.User, and EventGenres to be loaded.
        /// </summary>
        public static EventDto ToDto(this Event @event) => new EventDto
        {
            Id = @event.Id,
            Name = @event.Name,
            About = @event.About,
            Price = @event.Price,
            TotalTickets = @event.TotalTickets,
            AvailableTickets = @event.AvailableTickets,
            DatePosted = @event.DatePosted,
            StartDate = @event.Application?.Listing?.StartDate ?? default,
            EndDate = @event.Application?.Listing?.EndDate ?? default,
            Artist = @event.Application?.Artist?.ToDto(),
            Venue = @event.Application?.Listing?.Venue?.ToDto(),
            Genres = @event.EventGenres?.Select(eg => eg.Genre.ToDto()).ToList() ?? new List<GenreDto>()
        };

        /// <summary>
        /// Maps DTO fields that live directly on the Event entity.
        /// StartDate/EndDate live on the Listing; Artist/Venue are navigation properties managed separately.
        /// </summary>
        public static Event ToEntity(this EventDto dto) => new Event
        {
            Id = dto.Id,
            Name = dto.Name,
            About = dto.About,
            Price = dto.Price,
            TotalTickets = dto.TotalTickets,
            AvailableTickets = dto.AvailableTickets,
            DatePosted = dto.DatePosted
        };
    }
}
