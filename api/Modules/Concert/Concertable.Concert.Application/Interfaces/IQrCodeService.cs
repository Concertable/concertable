namespace Concertable.Concert.Application.Interfaces;

internal interface IQrCodeService
{
    byte[] GenerateFromTicketId(Guid id);
    Task<byte[]> GetByTicketIdAsync(Guid ticketId);
}
