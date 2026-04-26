using Concertable.Payment.Application.Requests;
using FluentResults;

namespace Concertable.Payment.Application.Interfaces;

internal interface IPaymentService
{
    Task<Result<PaymentResponse>> ProcessAsync(TransactionRequest request);
}
