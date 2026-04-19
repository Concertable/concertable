using Concertable.Application.Interfaces;

namespace Concertable.Application.DTOs;

public record ConcertDto : IDetails
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public string? BannerUrl { get; set; }
    public string? Avatar { get; set; }
    public double Rating { get; set; }
    public decimal Price { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DatePosted { get; set; }
    public required ConcertVenueDto Venue { get; set; }
    public required ConcertArtistDto Artist { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}

public record ConcertVenueDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double Rating { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public record ConcertArtistDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Avatar { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
    public double Rating { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}

public record ConcertSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DatePosted { get; set; }
    public required ConcertVenueSummaryDto Venue { get; set; }
    public required ConcertArtistSummaryDto Artist { get; set; }
}

public record ConcertVenueSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double Rating { get; set; }
}

public record ConcertArtistSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double Rating { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}

public record TicketConcertDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string VenueName { get; set; }
    public required string ArtistName { get; set; }
}

