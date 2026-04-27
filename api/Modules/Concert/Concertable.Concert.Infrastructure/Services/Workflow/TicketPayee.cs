using System.Collections.Frozen;
using Concertable.Contract.Contracts;

namespace Concertable.Concert.Infrastructure.Services.Application;

internal sealed class TicketPayee : ITicketPayee
{
    private readonly FrozenDictionary<ContractType, ITicketPayee> payees;

    public TicketPayee(ArtistTicketPayee artist, VenueTicketPayee venue)
    {
        payees = new Dictionary<ContractType, ITicketPayee>
        {
            [ContractType.VenueHire] = artist,
            [ContractType.FlatFee] = venue,
            [ContractType.DoorSplit] = venue,
            [ContractType.Versus] = venue,
        }.ToFrozenDictionary();
    }

    public Guid Resolve(ConcertEntity concert, IContract contract) =>
        payees[contract.ContractType].Resolve(concert, contract);
}
