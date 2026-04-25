using Concertable.Contract.Abstractions;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal sealed class ContractLookup(
    IOpportunityApplicationRepository applicationRepository,
    IConcertBookingRepository bookingRepository,
    IConcertRepository concertRepository,
    IOpportunityRepository opportunityRepository,
    IContractModule contractModule) : IContractLookup
{
    private readonly Dictionary<int, IContract> cache = [];

    public async Task<IContract> GetByApplicationIdAsync(int applicationId)
    {
        var opportunityId = await applicationRepository.GetOpportunityIdAsync(applicationId)
            ?? throw new NotFoundException($"Application {applicationId} not found");
        return await GetByOpportunityIdAsync(opportunityId);
    }

    public async Task<IContract> GetByBookingIdAsync(int bookingId)
    {
        var opportunityId = await bookingRepository.GetOpportunityIdAsync(bookingId)
            ?? throw new NotFoundException($"Booking {bookingId} not found");
        return await GetByOpportunityIdAsync(opportunityId);
    }

    public async Task<IContract> GetByConcertIdAsync(int concertId)
    {
        var opportunityId = await concertRepository.GetOpportunityIdAsync(concertId)
            ?? throw new NotFoundException($"Concert {concertId} not found");
        return await GetByOpportunityIdAsync(opportunityId);
    }

    private async Task<IContract> GetByOpportunityIdAsync(int opportunityId)
    {
        var contractId = await opportunityRepository.GetContractIdAsync(opportunityId);
        if (cache.TryGetValue(contractId, out var cached))
            return cached;

        var contract = await contractModule.GetByIdAsync(contractId)
            ?? throw new NotFoundException($"No contract for opportunity {opportunityId}");
        cache[contractId] = contract;
        return contract;
    }
}
