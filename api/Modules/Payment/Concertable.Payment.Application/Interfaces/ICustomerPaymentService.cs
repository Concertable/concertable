using FluentResults;

namespace Concertable.Payment.Application.Interfaces;

internal interface ICustomerPaymentService
{
    Task<Result<PaymentResponse>> PayAsync(CustomerDto payer, ManagerDto payee, int concertId, int quantity, string? paymentMethodId, decimal price);
}
