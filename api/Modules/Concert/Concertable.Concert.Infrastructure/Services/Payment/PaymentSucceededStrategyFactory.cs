using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class PaymentSucceededStrategyFactory : IPaymentSucceededStrategyFactory
{
    private readonly IServiceProvider serviceProvider;

    public PaymentSucceededStrategyFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IPaymentSucceededStrategy Create(string type)
        => serviceProvider.GetRequiredKeyedService<IPaymentSucceededStrategy>(type);
}
