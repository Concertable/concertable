using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Validators;

internal class StripeValidationFactory : IStripeValidationFactory
{
    private readonly IServiceProvider serviceProvider;

    public StripeValidationFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IStripeValidationStrategy Create(ContractType contractType) =>
        serviceProvider.GetRequiredKeyedService<IStripeValidationStrategy>(contractType);
}
