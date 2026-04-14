using Concertable.Application.Requests;
using Concertable.Application.Responses;
using FluentResults;

namespace Concertable.Application.Interfaces.Payment;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> ProcessAsync(TransactionRequest request);
}
