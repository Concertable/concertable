using Concertable.Application.Requests;
using FluentValidation;

namespace Concertable.Application.Validators;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ConcertId).GreaterThan(0);
        RuleFor(x => x.Stars).InclusiveBetween((byte)1, (byte)5);
    }
}
