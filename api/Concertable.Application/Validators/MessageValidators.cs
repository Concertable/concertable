using Concertable.Application.Requests;
using FluentValidation;

namespace Concertable.Application.Validators;

public class MarkMessagesReadRequestValidator : AbstractValidator<MarkMessagesReadRequest>
{
    public MarkMessagesReadRequestValidator()
    {
        RuleFor(x => x.MessageIds).NotEmpty().WithMessage("Require one MessageId minimum.");
    }
}
