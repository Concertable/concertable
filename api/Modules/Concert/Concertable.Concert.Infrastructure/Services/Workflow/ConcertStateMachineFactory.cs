using Concertable.Concert.Application.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal sealed class ConcertStateMachineFactory : IConcertStateMachineFactory
{
    private readonly IKeyedServiceProvider serviceProvider;

    public ConcertStateMachineFactory(IKeyedServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IConcertStateMachine Create(ContractType contractType) =>
        serviceProvider.GetRequiredKeyedService<IConcertStateMachine>(contractType);
}
