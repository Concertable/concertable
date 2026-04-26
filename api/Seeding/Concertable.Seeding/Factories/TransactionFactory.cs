using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class TransactionFactory
{
    public static SettlementTransactionEntity Settlement(Guid fromUserId, Guid toUserId, string paymentIntentId, long amount, TransactionStatus status, int bookingId)
        => New<SettlementTransactionEntity>()
            .With(nameof(TransactionEntity.FromUserId), fromUserId)
            .With(nameof(TransactionEntity.ToUserId), toUserId)
            .With(nameof(TransactionEntity.PaymentIntentId), paymentIntentId)
            .With(nameof(TransactionEntity.Amount), amount)
            .With(nameof(TransactionEntity.Status), status)
            .With(nameof(SettlementTransactionEntity.BookingId), bookingId);

    public static TicketTransactionEntity Ticket(Guid fromUserId, Guid toUserId, string paymentIntentId, long amount, TransactionStatus status, int concertId)
        => New<TicketTransactionEntity>()
            .With(nameof(TransactionEntity.FromUserId), fromUserId)
            .With(nameof(TransactionEntity.ToUserId), toUserId)
            .With(nameof(TransactionEntity.PaymentIntentId), paymentIntentId)
            .With(nameof(TransactionEntity.Amount), amount)
            .With(nameof(TransactionEntity.Status), status)
            .With(nameof(TicketTransactionEntity.ConcertId), concertId);
}
