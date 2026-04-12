using Concertable.Application.Results;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface ICustomerPaymentService
{
    Task<PaymentResult> PayAsync(CustomerEntity payer, ManagerEntity payee, int concertId, int quantity, string? paymentMethodId, decimal price);
}
