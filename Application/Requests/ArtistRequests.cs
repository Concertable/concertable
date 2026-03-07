using Application.DTOs;
using Application.Validators;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Requests
{
    public record CreateArtistRequest
    {
        public string Name { get; set; }
        public string About { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; }
    }

    public record ArtistCreateRequest
    {
        [Required]
        public CreateArtistRequest Artist { get; set; }
        [Required]
        public IFormFile Image { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!ImageValidator.Validate(Image, out var error))
                yield return new ValidationResult(error, new[] { nameof(Image) });
        }
    }

    public record ArtistUpdateRequest
    {
        public ArtistDto Artist { get; set; }
        public IFormFile? Image { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Image is not null && !ImageValidator.Validate(Image, out var error))
                yield return new ValidationResult(error, new[] { nameof(Image) });
        }
    }
}
