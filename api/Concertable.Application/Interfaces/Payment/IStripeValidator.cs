using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Payment;

public interface IStripeValidator
{
    Task<ValidationResult> ValidateUserAsync();
}
