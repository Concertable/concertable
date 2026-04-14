using Concertable.Application.Responses;
using Concertable.Core.Entities;
using FluentResults;

namespace Concertable.Application.Interfaces.Payment;

public interface ICustomerPaymentService
{
    Task<Result<PaymentResponse>> PayAsync(CustomerEntity payer, ManagerEntity payee, int concertId, int quantity, string? paymentMethodId, decimal price);
}
