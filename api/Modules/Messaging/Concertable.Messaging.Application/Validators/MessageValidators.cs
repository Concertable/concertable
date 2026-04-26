using Concertable.Messaging.Application.Requests;
using FluentValidation;

namespace Concertable.Messaging.Application.Validators;

internal class MarkMessagesReadRequestValidator : AbstractValidator<MarkMessagesReadRequest>
{
    public MarkMessagesReadRequestValidator()
    {
        RuleFor(x => x.MessageIds).NotEmpty().WithMessage("Require one MessageId minimum.");
    }
}
