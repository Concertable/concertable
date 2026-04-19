using Concertable.Application.Requests;
using Concertable.Core.Parameters;
using FluentValidation;

namespace Concertable.Application.Validators;

public class UpdateConcertRequestValidator : AbstractValidator<UpdateConcertRequest>
{
    public UpdateConcertRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.About).MaximumLength(1000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalTickets).GreaterThanOrEqualTo(0);
    }
}


public class ConcertParamsValidator : AbstractValidator<ConcertParams>
{
    public ConcertParamsValidator()
    {
        Include(new Parameters.GeoParamsValidator());
        RuleFor(x => x.Take).GreaterThan(0).When(x => x.Take != 0);
    }
}

public class ConcertBookingParamsValidator : AbstractValidator<ConcertBookingParams>
{
    public ConcertBookingParamsValidator()
    {
        RuleFor(x => x.PaymentMethodId).NotEmpty().WithMessage("Payment method ID is required");
        RuleFor(x => x.ApplicationId).GreaterThan(0).WithMessage("Application ID is required");
    }
}
