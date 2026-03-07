using Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public record ItemDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
        public string About { get; set; }
        public string Type { get; set; }
        public double Rating { get; set; }
    }

    public record UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string? Role { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? County { get; set; }
        public string? Town { get; set; }
        public string BaseUrl { get; set; } = "/";
    }

    public record GenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public record ActionDto
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public record LocationDto
    {
        public string County { get; set; }
        public string Town { get; set; }
    }

    public record AttachmentDto
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; } = "application/pdf";
    }

    public record EmailDto
    {
        public required string To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public IEnumerable<AttachmentDto> Attachments { get; set; }
    }

    public record CoordinatesDto
    {
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
        public double Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
        public double Longitude { get; set; }
    }

    public record TicketDto
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; }
        public byte[]? QrCode { get; set; }
        public EventDto Event { get; set; }
        public UserDto User { get; set; }
    }
}
