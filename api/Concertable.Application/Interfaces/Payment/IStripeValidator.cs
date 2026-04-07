namespace Concertable.Application.Interfaces.Payment;

public interface IStripeValidator
{
    Task<bool> ValidateAccountAsync();
    Task<bool> ValidateCustomerAsync();
}
