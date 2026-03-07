using Application.Requests;
using FluentValidation;

namespace Application.Validators
{
    public class CreatePreferenceRequestValidator : AbstractValidator<CreatePreferenceRequest>
    {
        public CreatePreferenceRequestValidator()
        {
            RuleFor(x => x.RadiusKm).GreaterThan(0);
        }
    }
}
