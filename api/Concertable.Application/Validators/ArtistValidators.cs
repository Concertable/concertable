using Concertable.Application.Requests;
using FluentValidation;

namespace Concertable.Application.Validators;

public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.About)
            .MaximumLength(1000);

        RuleFor(x => x.Banner)
            .NotNull()
            .SetValidator(new BannerImageValidator());
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

        When(x => x.Banner != null, () =>
        {
            RuleFor(x => x.Banner!.File)
                .SetValidator(new BannerImageValidator());
        });

        When(x => x.Avatar != null, () =>
        {
            RuleFor(x => x.Avatar!)
                .SetValidator(new AvatarImageValidator());
        });
    }
}
