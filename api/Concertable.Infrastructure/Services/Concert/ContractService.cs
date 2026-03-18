using Application.Interfaces.Concert;
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
        var entity = await contractRepository.GetByOpportunityIdAsync(opportunityId)
            ?? throw new NotFoundException("Contract not found for this opportunity");

        var mapper = mapperFactory.Create(entity.ContractType);
        return mapper.ToDto(entity);
    }

    public async Task CreateAsync(IContract contract)
    {
        var mapper = mapperFactory.Create(contract.ContractType);
        var entity = mapper.ToEntity(contract);
        await contractRepository.AddAsync(entity);
        await contractRepository.SaveChangesAsync();
    }
}
