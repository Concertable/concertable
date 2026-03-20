namespace Application.Interfaces.Concert;

public interface IAcceptProcessor
{
    Task AcceptAsync(int applicationId);
}
