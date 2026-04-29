using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class PaymentSucceededProcessorFactory : IPaymentSucceededProcessorFactory
{
    private readonly IKeyedServiceProvider serviceProvider;

    public PaymentSucceededProcessorFactory(IKeyedServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IPaymentSucceededProcessor Create(string type)
        => serviceProvider.GetRequiredKeyedService<IPaymentSucceededProcessor>(type);
}
