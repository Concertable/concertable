using Application.DTOs;
using Application.Validators;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests
{
    public class ArtistCreateRequest
    {
        [Required]
        public CreateArtistDto Artist { get; set; }
        [Required]
        public IFormFile Image { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!ImageValidator.Validate(Image, out var error))
                yield return new ValidationResult(error, new[] { nameof(Image) });
        }
    }
}
