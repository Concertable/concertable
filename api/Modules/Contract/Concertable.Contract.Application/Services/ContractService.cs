using Concertable.Contract.Application.Interfaces;
using Concertable.Shared.Exceptions;

namespace Concertable.Contract.Application.Services;

internal sealed class ContractService(IContractRepository contractRepository, IContractMapper mapper) : IContractService
{
    public async Task<IContract?> GetByIdAsync(int contractId, CancellationToken ct = default)
    {
        var entity = await contractRepository.GetByIdAsync(contractId);
        return entity is null ? null : mapper.ToContract(entity);
    }

    public async Task<int> CreateAsync(IContract contract, CancellationToken ct = default)
    {
        var entity = mapper.ToEntity(contract);
        await contractRepository.AddAsync(entity);
        await contractRepository.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(int contractId, IContract contract, CancellationToken ct = default)
    {
        var existing = await contractRepository.GetByIdAsync(contractId)
            ?? throw new NotFoundException($"Contract {contractId} not found");

        Apply(existing, contract);
        contractRepository.Update(existing);
        await contractRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int contractId, CancellationToken ct = default)
    {
        var existing = await contractRepository.GetByIdAsync(contractId);
        if (existing is null) return;

        contractRepository.Remove(existing);
        await contractRepository.SaveChangesAsync();
    }

    private static void Apply(ContractEntity existing, IContract source)
    {
        switch (existing, source)
        {
            case (FlatFeeContractEntity e, FlatFeeContract c):
                e.Update(c.Fee, c.PaymentMethod);
                break;
            case (DoorSplitContractEntity e, DoorSplitContract c):
                e.Update(c.ArtistDoorPercent, c.PaymentMethod);
                break;
            case (VersusContractEntity e, VersusContract c):
                e.Update(c.Guarantee, c.ArtistDoorPercent, c.PaymentMethod);
                break;
            case (VenueHireContractEntity e, VenueHireContract c):
                e.Update(c.HireFee, c.PaymentMethod);
                break;
            default:
                throw new InvalidOperationException(
                    $"Cannot apply {source.GetType().Name} to {existing.GetType().Name}");
        }
    }
}
