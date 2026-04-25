using Concertable.Contract.Abstractions;
using FluentResults;

namespace Concertable.Payment.Application.Interfaces;

internal interface ITicketPaymentStrategy : IContractStrategy
{
    Task<Result<PaymentResponse>> PayAsync(
        int concertId, int quantity, string? paymentMethodId, decimal price,
        Guid customerUserId, Guid payeeUserId,
        IContract contract);
}
