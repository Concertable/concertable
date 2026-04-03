using Concertable.Application.DTOs;
using FluentValidation;

namespace Concertable.Application.Validators;

public class OpportunityDtoValidator : AbstractValidator<OpportunityDto>
{
    public OpportunityDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("You cannot create a Concert Opportunity in the past.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x.EndDate)
            .Must((dto, endDate) => (endDate - dto.StartDate).TotalHours <= 24)
            .WithMessage("EndDate can be at most 24 hours after StartDate.")
            .When(x => x.EndDate > x.StartDate);

    }
}
