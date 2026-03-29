using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Factories;

public class ContractStrategyResolver<T> : IContractStrategyResolver<T> where T : IContractStrategy
{
    private readonly IContractRepository contractRepository;
    private readonly IContractStrategyFactory<T> factory;

    public ContractStrategyResolver(
        IContractRepository contractRepository,
        IContractStrategyFactory<T> factory)
    {
        this.contractRepository = contractRepository;
        this.factory = factory;
    }

    public async Task<T> ResolveForConcertAsync(int concertId)
    {
        var contractType = await contractRepository.GetTypeByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Contract not found for this concert");
        return factory.Create(contractType);
    }

    public async Task<T> ResolveForApplicationAsync(int applicationId)
    {
        var contractType = await contractRepository.GetTypeByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Contract not found for this application");
        return factory.Create(contractType);
    }
}
