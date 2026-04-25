namespace Concertable.Concert.Contracts;

public interface ITicketPaymentModule
{
    Task<IReadOnlyList<Guid>> IssueTicketsAsync(int concertId, Guid userId, int quantity, CancellationToken ct = default);
    Task RefundTicketAsync(Guid ticketId, CancellationToken ct = default);
}
