using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Mappers;
using Core.Exceptions;

namespace Infrastructure.Services.Concert;

public class BookingContractService : IBookingContractService
{
    private readonly IBookingContractRepository contractRepository;

    public BookingContractService(IBookingContractRepository contractRepository)
    {
        this.contractRepository = contractRepository;
    }

    public async Task<IBookingContract> GetByOpportunityIdAsync(int opportunityId)
    {
        var contract = await contractRepository.GetByOpportunityIdAsync(opportunityId)
            ?? throw new NotFoundException("Contract not found for this opportunity");

        return contract.ToDto();
    }

    public async Task CreateAsync(IBookingContract contract)
    {
        var entity = contract.ToEntity();
        await contractRepository.AddAsync(entity);
        await contractRepository.SaveChangesAsync();
    }
}
