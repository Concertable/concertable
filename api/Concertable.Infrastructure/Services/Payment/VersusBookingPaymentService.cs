using Application.Interfaces.Concert;
using Application.Responses;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

public class VersusBookingPaymentService : IBookingPaymentStrategy
{
    public Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId)
        => throw new BadRequestException("Versus contracts do not require a booking payment. Payouts are calculated and settled post-event.");
}
