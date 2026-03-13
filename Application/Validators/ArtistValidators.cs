using Application.Requests;
using FluentValidation;

namespace Application.Validators;

public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.About)
            .MaximumLength(1000);

        RuleFor(x => x.Image)
            .NotNull()
            .SetValidator(new IFormFileValidator());
    }
}

public class UpdateArtistRequestValidator : AbstractValidator<UpdateArtistRequest>
{
    public UpdateArtistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.About)
            .MaximumLength(1000);

        RuleFor(x => x.ImageUrl).NotEmpty();

        When(x => x.Image != null, () =>
        {
            RuleFor(x => x.Image!)
                .SetValidator(new IFormFileValidator());
        });
    }
}
