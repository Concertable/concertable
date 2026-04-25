using Concertable.Payment.Application.Requests;
using FluentValidation;

namespace Concertable.Payment.Application.Validators;

internal class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public TransactionRequestValidator()
    {
        RuleFor(x => x.PaymentMethodId).NotEmpty();
        RuleFor(x => x.FromUserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Metadata).NotNull().NotEmpty();
    }
}
