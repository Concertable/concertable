using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Concert;

public class ContractService : IContractService
{
    private readonly IContractRepository contractRepository;
    private readonly IContractMapper contractMapper;
    private readonly IContractServiceStrategy contractServiceStrategy;

    public ContractService(
        IContractRepository contractRepository,
        IContractMapper contractMapper,
        IContractServiceStrategy contractServiceStrategy)
    {
        this.contractRepository = contractRepository;
        this.contractMapper = contractMapper;
        this.contractServiceStrategy = contractServiceStrategy;
    }

    public async Task<IContract> GetByOpportunityIdAsync(int opportunityId)
    {
        var entity = await contractRepository.GetByOpportunityIdAsync<ContractEntity>(opportunityId)
            ?? throw new NotFoundException("Contract not found for this opportunity");

        return contractMapper.ToDto(entity);
    }

    public async Task AddAsync(IContract contract, int opportunityId)
    {
        var entity = contractMapper.ToEntity(contract);
        entity.Id = opportunityId;
        await contractRepository.AddAsync(entity);
    }

    public async Task CreateAsync(IContract contract, int opportunityId)
    {
        await AddAsync(contract, opportunityId);
        await contractRepository.SaveChangesAsync();
    }

    public async Task UpdateAsync(IContract contract, int opportunityId)
    {
        var existing = await contractRepository.GetByOpportunityIdAsync<ContractEntity>(opportunityId)
            ?? throw new NotFoundException("Contract not found for this opportunity");

        if (existing.ContractType != contract.ContractType)
            throw new BadRequestException("Contract type cannot be changed when updating an opportunity");

        contractServiceStrategy.ApplyChanges(existing, contract);
    }


}
