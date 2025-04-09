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
        public double Pay { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult("EndDate must be after StartDate", [nameof(EndDate)]);
                yield break;
            }

            if ((EndDate - StartDate).TotalHours > 24)
                yield return new ValidationResult("EndDate can be at most 24 hours after StartDate", [nameof(EndDate)]);
        }
    }
}
