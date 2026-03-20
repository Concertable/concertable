using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Exceptions;

namespace Infrastructure.Factories;

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
        var contract = await contractRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Contract not found for this concert");
        return factory.Create(contract.ContractType);
    }

    public async Task<T> ResolveForApplicationAsync(int applicationId)
    {
        var contract = await contractRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("Contract not found for this application");
        return factory.Create(contract.ContractType);
    }
}
