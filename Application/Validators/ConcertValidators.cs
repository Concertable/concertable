using Application.Requests;
using FluentValidation;

namespace Application.Validators;

public class UpdateConcertRequestValidator : AbstractValidator<UpdateConcertRequest>
{
    public UpdateConcertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.About)
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TotalTickets)
            .GreaterThanOrEqualTo(0);
    }
}
