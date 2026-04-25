using Concertable.Application.Interfaces;
using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Contracts;
using Concertable.Identity.Contracts;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure;

internal sealed class TicketPaymentModule(
    ITicketService ticketService,
    ICustomerModule customerModule,
    ITicketNotificationService notificationService) : ITicketPaymentModule
{
    public async Task<IReadOnlyList<Guid>> IssueTicketsAsync(int concertId, Guid userId, int quantity, CancellationToken ct = default)
    {
        var customer = await customerModule.GetCustomerAsync(userId)
            ?? throw new NotFoundException($"Customer {userId} not found");

        var result = await ticketService.CompleteAsync(new()
        {
            EntityId = concertId,
            FromUserId = userId,
            FromEmail = customer.Email ?? string.Empty,
            Quantity = quantity,
            TransactionId = string.Empty
        });

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        await notificationService.TicketPurchasedAsync(userId.ToString(), result.Value);

        return [.. result.Value.TicketIds];
    }

    public Task RefundTicketAsync(Guid ticketId, CancellationToken ct = default)
        => throw new NotImplementedException("Ticket refund flow is backlog — placeholder added at Payment refactor Step 4 to lock ownership.");
}
