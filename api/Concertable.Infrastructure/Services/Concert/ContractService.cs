using Concertable.Application.Interfaces.Concert;
using Concertable.Shared.Exceptions;

namespace Concertable.Infrastructure.Services.Concert;

public class ContractService : IContractService
{
    private readonly IContractRepository contractRepository;
    private readonly IContractMapper contractMapper;

    public ContractService(IContractRepository contractRepository, IContractMapper contractMapper)
    {
        this.contractRepository = contractRepository;
        this.contractMapper = contractMapper;
    }

    public async Task<IContract> GetByOpportunityIdAsync(int opportunityId)
    {
        var entity = await contractRepository.GetByOpportunityIdAsync<ContractEntity>(opportunityId)
            ?? throw new NotFoundException("Contract not found for this opportunity");

        return contractMapper.ToDto(entity);
    }
}
