using Application.DTOs;
using Application.Validators;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Requests
{
    public record CreateVenueRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(1, ErrorMessage = "Name cannot be empty.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
        public string About { get; set; }

        [Required(ErrorMessage = "Coordinates are required.")]
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public record VenueCreateRequest
    {
        [Required]
        public CreateVenueRequest Venue { get; set; }
        [Required]
        public IFormFile Image { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!ImageValidator.Validate(Image, out var error))
                yield return new ValidationResult(error, new[] { nameof(Image) });
        }
    }

    public record VenueUpdateRequest : IValidatableObject
    {
        public VenueDto Venue { get; set; }
        public IFormFile? Image { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Image is not null && !ImageValidator.Validate(Image, out var error))
                yield return new ValidationResult(error, new[] { nameof(Image) });
        }
    }
}
