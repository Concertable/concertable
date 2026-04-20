using Concertable.Application.Responses;
using FluentResults;

namespace Concertable.Application.Interfaces.Payment;

public interface ICustomerPaymentService
{
    Task<Result<PaymentResponse>> PayAsync(CustomerDto payer, ManagerDto payee, int concertId, int quantity, string? paymentMethodId, decimal price);
}
