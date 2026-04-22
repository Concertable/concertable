using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketPaymentStrategy : IContractStrategy
{
    Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price);
}
