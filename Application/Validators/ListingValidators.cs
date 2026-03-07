using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class ListingDtoValidator : AbstractValidator<ListingDto>
    {
        public ListingDtoValidator()
        {
            RuleFor(x => x.StartDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("You cannot create a Listing in the past.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate.");

            RuleFor(x => x.EndDate)
                .Must((dto, endDate) => (endDate - dto.StartDate).TotalHours <= 24)
                .WithMessage("EndDate can be at most 24 hours after StartDate.")
                .When(x => x.EndDate > x.StartDate);

            RuleFor(x => x.Pay)
                .GreaterThanOrEqualTo(0).WithMessage("Pay cannot be negative.")
                .Must(pay => decimal.Round(pay, 2) == pay).WithMessage("Pay must have exactly two decimal places.");
        }
    }
}
