using Concertable.Payment.Application.Requests;
using Concertable.Payment.Application.Responses;
using FluentResults;

namespace Concertable.Payment.Application.Interfaces;

internal interface IStripePaymentIntentClient
{
    Task<Result<PaymentResponse>> ChargeAsync(StripeChargeOptions options);
    Task<Result<PaymentResponse>> HoldAsync(StripeHoldOptions options);
    Task<Result<TransferResponse>> ReleaseAsync(StripeReleaseOptions options);
    Task<Result<RefundResponse>> RefundAsync(StripeRefundOptions options);
}
