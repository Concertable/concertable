using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Validators;

public class TicketPurchaseParamsValidator : AbstractValidator<TicketPurchaseParams>
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
