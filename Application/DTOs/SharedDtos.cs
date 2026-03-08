using Common.Helpers;

namespace Application.DTOs;

public record ItemDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Rating { get; set; }
}

public record UserDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public string? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public string BaseUrl { get; set; } = "/";
}

public record GenreDto(int Id, string Name);

public record ActionDto(string Name, int Id);

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

public record TicketDto
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public byte[]? QrCode { get; set; }
    public required ConcertDto Concert { get; set; }
    public required UserDto User { get; set; }
}
