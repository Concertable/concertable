using Application.Responses;

namespace Application.Interfaces.Concert;

public interface ITicketPaymentService : IContractWorkflow
{
    Task<PaymentResponse> PayAsync(int concertId, int quantity, string paymentMethodId, decimal price);
}
