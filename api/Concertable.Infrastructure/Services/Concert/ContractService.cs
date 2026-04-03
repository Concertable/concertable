using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Infrastructure.Services.Concert;

public class ContractService : IContractService
{
    private readonly IContractRepository contractRepository;
    private readonly IContractMapper contractMapper;

    public ContractService(
        IContractRepository contractRepository,
        IContractMapper contractMapper)
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

        var normalized = NormalizeContractId(contract, opportunityId);
        var mapped = contractMapper.ToEntity(normalized);

        existing.PaymentMethod = mapped.PaymentMethod;
        switch (existing, mapped)
        {
            case (FlatFeeContractEntity e, FlatFeeContractEntity m):
                e.Fee = m.Fee;
                break;
            case (DoorSplitContractEntity e, DoorSplitContractEntity m):
                e.ArtistDoorPercent = m.ArtistDoorPercent;
                break;
            case (VersusContractEntity e, VersusContractEntity m):
                e.Guarantee = m.Guarantee;
                e.ArtistDoorPercent = m.ArtistDoorPercent;
                break;
            case (VenueHireContractEntity e, VenueHireContractEntity m):
                e.HireFee = m.HireFee;
                break;
            default:
                throw new BadRequestException("Contract payload does not match the existing contract type");
        }
    }

    private static IContract NormalizeContractId(IContract contract, int opportunityId) => contract switch
    {
        FlatFeeContractDto f => f with { Id = opportunityId },
        DoorSplitContractDto d => d with { Id = opportunityId },
        VersusContractDto v => v with { Id = opportunityId },
        VenueHireContractDto h => h with { Id = opportunityId },
        _ => throw new BadRequestException("Unsupported contract type")
    };
}
