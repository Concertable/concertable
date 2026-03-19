using Application.Responses;
using Core.Enums;

namespace Application.Interfaces.Concert;

public interface ITicketPaymentService
{
    Task<PaymentResponse> PayAsync(int concertId, int quantity, string paymentMethodId, decimal price, ContractType contractType);
}
