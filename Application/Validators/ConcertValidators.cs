using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class ConcertDtoValidator : AbstractValidator<ConcertDto>
    {
        public ConcertDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
            RuleFor(x => x.TotalTickets).GreaterThan(0).WithMessage("Total tickets must be greater than 0.");
            RuleFor(x => x.AvailableTickets).GreaterThanOrEqualTo(0).WithMessage("Available tickets cannot be negative.");
            RuleFor(x => x.StartDate).NotEmpty().GreaterThan(DateTime.UtcNow).WithMessage("You cannot have a Concert in the past.");
            RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
            RuleFor(x => x.Venue).NotNull();
            RuleFor(x => x.Artist).NotNull();
            RuleFor(x => x.Artist.ImageUrl).NotEmpty().WithMessage("Artist image is required.").When(x => x.Artist != null);
        }
    }
}
