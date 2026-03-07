using Application.DTOs;
using Application.Requests;
using FluentValidation;

namespace Application.Validators
{
    public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
    {
        public CreateArtistRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.About).MaximumLength(1000);
        }
    }

    public class ArtistCreateRequestValidator : AbstractValidator<ArtistCreateRequest>
    {
        public ArtistCreateRequestValidator()
        {
            RuleFor(x => x.Artist).NotNull();
            RuleFor(x => x.Image)
                .NotNull()
                .Custom((image, context) =>
                {
                    if (image != null && !ImageValidator.Validate(image, out var error))
                        context.AddFailure(error!);
                });
        }
    }

    public class ArtistUpdateRequestValidator : AbstractValidator<ArtistUpdateRequest>
    {
        public ArtistUpdateRequestValidator()
        {
            RuleFor(x => x.Image)
                .Custom((image, context) =>
                {
                    if (image != null && !ImageValidator.Validate(image, out var error))
                        context.AddFailure(error!);
                });
        }
    }

    public class ArtistDtoValidator : AbstractValidator<ArtistDto>
    {
        public ArtistDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.About).MaximumLength(1000);
        }
    }
}
