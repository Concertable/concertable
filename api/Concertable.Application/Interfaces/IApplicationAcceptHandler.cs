using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IApplicationAcceptHandler
{
    Task HandleAsync(int applicationId, ConcertEntity concert);
}
