using Concertable.Payment.Application.Requests;
using FluentResults;

namespace Concertable.Payment.Application.Interfaces;

internal interface IPaymentManager
{
    Task<Result<PaymentResponse>> ChargeAsync(ChargeRequest request, CancellationToken ct = default);
}
