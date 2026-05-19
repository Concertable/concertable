namespace Concertable.DataAccess;

public interface IPdfService
{
    Task<byte[]> GenerateTicketReciptAsync(string email, Guid ticketId);
}
