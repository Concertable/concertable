using Application.Responses;

namespace Application.Interfaces;

public interface IStripeValidator
{
    Task<ValidationResult> ValidateUserAsync();
}
