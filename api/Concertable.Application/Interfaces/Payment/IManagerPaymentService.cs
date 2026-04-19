using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface IManagerPaymentService
{
    Task<PaymentResponse> PayAsync(ManagerEntity payer, ManagerEntity payee, decimal amount, int applicationId, string? paymentMethodId = null);
}
