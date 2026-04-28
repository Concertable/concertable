using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Payment;

internal class PaymentSucceededProcessorFactory : IPaymentSucceededProcessorFactory
{
    private readonly IServiceProvider serviceProvider;

    public PaymentSucceededProcessorFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IPaymentSucceededProcessor Create(string type)
        => serviceProvider.GetRequiredKeyedService<IPaymentSucceededProcessor>(type);
}
