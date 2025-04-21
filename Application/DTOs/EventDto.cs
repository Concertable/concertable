using Application.DTOs;
using System.ComponentModel.DataAnnotations;

public class EventDto : ItemDto, IValidatableObject
{

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Total tickets are required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Total tickets must be greater than 0.")]
    public int TotalTickets { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Available tickets cannot be negative.")]
    public int AvailableTickets { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }

    public DateTime? DatePosted { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    public VenueDto Venue { get; set; }

    [Required(ErrorMessage = "Artist is required.")]
    public ArtistDto Artist { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Artist != null && string.IsNullOrWhiteSpace(Artist.ImageUrl))
            yield return new ValidationResult("Artist image is required.", new[] { "Artist.ImageUrl" });

        if (StartDate != default && EndDate != default && StartDate >= EndDate)
            yield return new ValidationResult("End date must be after start date.", new[] { nameof(EndDate) });

        if (StartDate <= DateTime.UtcNow)
            yield return new ValidationResult("You cannot have an Event in the past", [nameof(EndDate)]);
    }

    public EventDto()
    {
        Type = "event";
    }
}
