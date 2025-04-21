using Application.Serializers;
using Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs
{
    public class ListingDto : IValidatableObject
    {
        public int? Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();
        [Range(0, double.MaxValue, ErrorMessage = "Pay cannot be negative")]
        public decimal Pay { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate <= DateTime.UtcNow)
                yield return new ValidationResult("You cannot create a Listing in the past", [nameof(EndDate)]);

            if (EndDate <= StartDate)
            {
                yield return new ValidationResult("EndDate must be after StartDate", [nameof(EndDate)]);
                yield break;
            }

            if ((EndDate - StartDate).TotalHours > 24)
                yield return new ValidationResult("EndDate can be at most 24 hours after StartDate", [nameof(EndDate)]);

            if (decimal.Round(Pay, 2) != Pay)
                yield return new ValidationResult("Pay must have exactly two decimal places", [nameof(Pay)]);
        }
    }
}
