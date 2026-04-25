using Concertable.Contract.Abstractions;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal sealed class ContractLookup(
    IOpportunityApplicationRepository applicationRepository,
    IConcertBookingRepository bookingRepository,
    IConcertRepository concertRepository,
    IContractModule contractModule) : IContractLookup
{
    private enum Kind { Application, Booking, Concert }
    private readonly record struct Key(Kind Kind, int Id);

    private readonly Dictionary<Key, IContract> cache = [];

    public Task<IContract> GetByApplicationIdAsync(int applicationId) =>
        ResolveAsync(new Key(Kind.Application, applicationId), applicationRepository.GetContractIdByApplicationIdAsync);

    public Task<IContract> GetByBookingIdAsync(int bookingId) =>
        ResolveAsync(new Key(Kind.Booking, bookingId), bookingRepository.GetContractIdByBookingIdAsync);

    public Task<IContract> GetByConcertIdAsync(int concertId) =>
        ResolveAsync(new Key(Kind.Concert, concertId), concertRepository.GetContractIdByConcertIdAsync);

    private async Task<IContract> ResolveAsync(Key key, Func<int, Task<int?>> resolveContractId)
    {
        if (cache.TryGetValue(key, out var hit))
            return hit;

        var contractId = await resolveContractId(key.Id)
            ?? throw new NotFoundException($"{key.Kind} {key.Id} not found");
        var contract = await contractModule.GetByIdAsync(contractId)
            ?? throw new NotFoundException($"No contract with id {contractId}");

        cache[key] = contract;
        return contract;
    }
}
