using Concertable.Core.Enums;
using Concertable.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Concertable.Application.DTOs;

public record UserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
}

public record GenreDto(int Id, string Name);

public class AutocompleteDto : IHasName
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    [JsonPropertyName("$type")]
    public required string Type { get; init; }
}

public record LocationDto(string County, string Town);

public record AttachmentDto
{
    public required byte[] Content { get; set; }
    public required string FileName { get; set; }
    public string MimeType { get; set; } = "application/pdf";
}

public record EmailDto
{
    public required string To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public IEnumerable<AttachmentDto> Attachments { get; set; } = [];
}

public record CoordinatesDto(double Latitude, double Longitude);

public record ImageDto
{
    public required string Url { get; init; }
    public required IFormFile File { get; init; }
}

public record TicketDto
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public byte[]? QrCode { get; set; }
    public required TicketConcertDto Concert { get; set; }
    public required UserDto User { get; set; }
}
