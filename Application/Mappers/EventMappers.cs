using Application.DTOs;
using Core.Entities;

namespace Application.Mappers
{
    public static class EventMappers
    {
        public static EventDto ToDto(this Event @event) => new()
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
            Venue = @event.Application?.Listing?.Venue?.ToDto(),
            Artist = @event.Application?.Artist?.ToDto(),
            Genres = @event.EventGenres?.Select(eg => eg.Genre.ToDto()) ?? Enumerable.Empty<GenreDto>()
        };

        public static EventHeaderDto ToHeaderDto(this Event @event) => new()
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

        public static EventHeaderDto ToHeaderDto(this EventDto eventDto) => new()
        {
            Id = eventDto.Id,
            Name = eventDto.Name,
            ImageUrl = eventDto.Artist?.ImageUrl,
            StartDate = eventDto.StartDate,
            EndDate = eventDto.EndDate,
            County = eventDto.Venue?.County,
            Town = eventDto.Venue?.Town,
            Latitude = eventDto.Venue?.Latitude,
            Longitude = eventDto.Venue?.Longitude,
            DatePosted = eventDto.DatePosted
        };

public static IEnumerable<EventDto> ToDtos(this IEnumerable<Event> events) =>
            events.Select(e => e.ToDto());

        public static IEnumerable<EventHeaderDto> ToHeaderDtos(this IEnumerable<Event> events) =>
            events.Select(e => e.ToHeaderDto());
    }
}
