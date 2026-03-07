using Application.DTOs;
using Application.Requests;
using FluentValidation;

namespace Application.Validators
{
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
                .NotNull()
                .InclusiveBetween(-90, 90);

            RuleFor(x => x.Longitude)
                .NotNull()
                .InclusiveBetween(-180, 180);
        }
    }

    public class VenueCreateRequestValidator : AbstractValidator<VenueCreateRequest>
    {
        public VenueCreateRequestValidator()
        {
            RuleFor(x => x.Venue).NotNull();
            RuleFor(x => x.Image)
                .NotNull()
                .Must(image => ImageValidator.Validate(image, out _))
                .WithMessage("Image must be a valid format (JPEG, PNG, GIF, BMP, WEBP) and under 5MB.");
        }
    }

    public class VenueUpdateRequestValidator : AbstractValidator<VenueUpdateRequest>
    {
        public VenueUpdateRequestValidator()
        {
            RuleFor(x => x.Image)
                .Must(image => image == null || ImageValidator.Validate(image, out _))
                .WithMessage("Image must be a valid format (JPEG, PNG, GIF, BMP, WEBP) and under 5MB.");
        }
    }

    public class VenueDtoValidator : AbstractValidator<VenueDto>
    {
        public VenueDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.About)
                .MaximumLength(1000);

            RuleFor(x => x.ImageUrl).NotEmpty();

            RuleFor(x => x.County)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Town)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180);
        }
    }
}
