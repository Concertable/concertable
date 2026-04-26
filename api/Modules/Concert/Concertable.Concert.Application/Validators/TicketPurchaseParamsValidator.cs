using FluentValidation;

namespace Concertable.Concert.Application.Validators;

internal class TicketPurchaseParamsValidator : AbstractValidator<TicketPurchaseParams>
{
    public TicketPurchaseParamsValidator()
    {
        RuleFor(x => x.ConcertId)
            .GreaterThan(0)
            .WithMessage("Concert ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be at least 1");
    }
}
