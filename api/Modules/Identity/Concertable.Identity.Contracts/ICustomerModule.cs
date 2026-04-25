namespace Concertable.Identity.Contracts;

public interface ICustomerModule
{
    Task<CustomerDto?> GetCustomerAsync(Guid userId);
}
