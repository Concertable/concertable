namespace Concertable.Application.Interfaces;

public interface IApplicationAcceptHandler
{
    Task HandleAsync(int applicationId);
}
