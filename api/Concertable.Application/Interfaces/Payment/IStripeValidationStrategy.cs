namespace Concertable.Application.Interfaces.Payment;

public interface IStripeValidationStrategy
{
    Task<bool> ValidateAsync();
}
