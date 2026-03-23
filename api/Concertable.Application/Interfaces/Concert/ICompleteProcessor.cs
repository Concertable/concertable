namespace Application.Interfaces.Concert;

public interface ICompleteProcessor
{
    Task CompleteAsync(int concertId);
}
