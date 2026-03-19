using Application.Interfaces.Concert;
using Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories;

public class PaymentRecipientResolverFactory : IPaymentRecipientResolverFactory
{
    private readonly IServiceProvider serviceProvider;

    public PaymentRecipientResolverFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IPaymentRecipientResolver Create(ContractType contractType) =>
        serviceProvider.GetRequiredKeyedService<IPaymentRecipientResolver>(contractType);
}
