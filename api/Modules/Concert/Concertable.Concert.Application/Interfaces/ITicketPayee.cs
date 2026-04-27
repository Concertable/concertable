using Concertable.Contract.Contracts;

namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketPayee
{
    Guid Resolve(ConcertEntity concert, IContract contract);
}
