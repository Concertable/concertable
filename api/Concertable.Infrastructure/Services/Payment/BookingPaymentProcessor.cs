using Application.Interfaces;
using Application.Interfaces.Concert;
using Application.Responses;

namespace Infrastructure.Services.Payment;

public class BookingPaymentProcessor : IBookingPaymentProcessor
{
    private readonly IContractStrategyResolver<IBookingPaymentStrategy> resolver;

    public BookingPaymentProcessor(IContractStrategyResolver<IBookingPaymentStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task<PaymentResponse> PayAsync(int applicationId, string paymentMethodId)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        return await strategy.PayAsync(applicationId, paymentMethodId);
    }
}
