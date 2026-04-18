using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Payment;

public interface IManagerPaymentService
{
    Task PayAsync(ManagerEntity payer, ManagerEntity payee, decimal amount, int applicationId, string? paymentMethodId = null);
}
