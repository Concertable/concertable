namespace Concertable.Application.Interfaces.Concert;

public interface IFinishedProcessor
{
    Task FinishedAsync(int concertId);
}
