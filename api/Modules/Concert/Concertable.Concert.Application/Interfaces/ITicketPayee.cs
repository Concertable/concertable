using Concertable.Contract.Abstractions;

namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketPayee
{
    Guid Resolve(ConcertEntity concert, IContract contract);
}
