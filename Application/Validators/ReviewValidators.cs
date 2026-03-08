using Application.Requests;
using FluentValidation;

namespace Application.Validators
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x.ConcertId).GreaterThan(0);
            RuleFor(x => x.Stars).InclusiveBetween(1, 5);
        }
    }
}
