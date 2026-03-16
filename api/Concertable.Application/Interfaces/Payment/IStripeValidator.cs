using Application.Responses;

namespace Application.Interfaces.Payment;

public interface IStripeValidator
{
    Task<ValidationResult> ValidateUserAsync();
}
