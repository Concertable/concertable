using Concertable.Application.Requests;
using FluentValidation;

namespace Concertable.Application.Validators;

public class CreatePreferenceRequestValidator : AbstractValidator<CreatePreferenceRequest>
{
    public CreatePreferenceRequestValidator()
    {
        RuleFor(x => x.RadiusKm).GreaterThan(0);
    }
}
