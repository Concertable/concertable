using Concertable.Application.Responses;
using FluentResults;

namespace Concertable.Application.Interfaces.Concert;

public interface ITicketPaymentDispatcher
{
    Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price);
}
