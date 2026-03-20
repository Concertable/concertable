using Application.Interfaces.Concert;
using Application.Responses;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

public class DoorSplitBookingPaymentService : IBookingPaymentStrategy
{
    public Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId)
        => throw new BadRequestException("DoorSplit contracts do not require a booking payment. Revenue is split and settled post-event.");
}
