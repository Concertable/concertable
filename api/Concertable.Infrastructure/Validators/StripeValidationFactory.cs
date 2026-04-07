using Concertable.Application.Interfaces.Payment;
using Concertable.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Validators;

public class StripeValidationFactory : IStripeValidationFactory
{
    private readonly IServiceProvider serviceProvider;

    public StripeValidationFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IStripeValidationStrategy Create(ContractType contractType) =>
        serviceProvider.GetRequiredKeyedService<IStripeValidationStrategy>(contractType);
}
