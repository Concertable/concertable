using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Core.Exceptions;

namespace Infrastructure.Services.Concert;

public class ContractService : IContractService
{
    private readonly IContractRepository contractRepository;
    private readonly IContractMapperFactory mapperFactory;

    public ContractService(
        IContractRepository contractRepository,
        IContractMapperFactory mapperFactory)
    {
        this.contractRepository = contractRepository;
        this.mapperFactory = mapperFactory;
    }

    public async Task<IContract> GetByOpportunityIdAsync(int opportunityId)
    {
        var entity = await contractRepository.GetByOpportunityIdAsync<ContractEntity>(opportunityId)
            ?? throw new NotFoundException("Contract not found for this opportunity");

        var mapper = mapperFactory.Create(entity.ContractType);
        return mapper.ToDto(entity);
    }

    public async Task AddAsync(IContract contract, int opportunityId)
    {
        var mapper = mapperFactory.Create(contract.ContractType);
        var entity = mapper.ToEntity(contract);
        entity.Id = opportunityId;
        await contractRepository.AddAsync(entity);
    }

    public async Task CreateAsync(IContract contract, int opportunityId)
    {
        await AddAsync(contract, opportunityId);
        await contractRepository.SaveChangesAsync();
    }
}
