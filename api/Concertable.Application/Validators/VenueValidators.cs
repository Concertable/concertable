using Concertable.Application.Requests;
using FluentValidation;

namespace Concertable.Application.Validators;

public class CreateVenueRequestValidator : AbstractValidator<CreateVenueRequest>
{
    public CreateVenueRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.About)
            .MaximumLength(1000);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.Image)
            .NotNull()
            .SetValidator(new IFormFileValidator());
    }
}

public class UpdateVenueRequestValidator : AbstractValidator<UpdateVenueRequest>
{
    public UpdateVenueRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.About)
            .MaximumLength(1000);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        When(x => x.Image != null, () =>
        {
            RuleFor(x => x.Image!.File)
                .SetValidator(new IFormFileValidator());
        });
    }
}
