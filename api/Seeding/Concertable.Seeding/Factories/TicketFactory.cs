using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class TicketFactory
{
    public static TicketEntity Create(Guid id, Guid userId, int concertId, byte[] qrCode, DateTime purchaseDate)
        => New<TicketEntity>()
            .With(nameof(TicketEntity.Id), id)
            .With(nameof(TicketEntity.UserId), userId)
            .With(nameof(TicketEntity.ConcertId), concertId)
            .With(nameof(TicketEntity.QrCode), qrCode)
            .With(nameof(TicketEntity.PurchaseDate), purchaseDate);
}
