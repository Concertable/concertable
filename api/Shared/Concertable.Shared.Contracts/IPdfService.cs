namespace Concertable.Application.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateTicketReciptAsync(string email, Guid ticketId);
}
