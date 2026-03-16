using Application.Requests;
using FluentValidation;

namespace Application.Validators;

public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public TransactionRequestValidator()
    {
        RuleFor(x => x.PaymentMethodId).NotEmpty();
        RuleFor(x => x.FromUserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Metadata).NotNull().NotEmpty();
    }
}
