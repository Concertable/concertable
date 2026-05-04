using Concertable.Payment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Payment.Infrastructure.Repositories;

internal sealed class EscrowRepository(PaymentDbContext context)
    : Repository<EscrowEntity>(context), IEscrowRepository
{
    public Task<EscrowEntity?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default) =>
        context.Escrows.FirstOrDefaultAsync(e => e.BookingId == bookingId, ct);

    public Task<EscrowEntity?> GetByChargeIdAsync(string chargeId, CancellationToken ct = default) =>
        context.Escrows.FirstOrDefaultAsync(e => e.ChargeId == chargeId, ct);

    public async Task<IReadOnlyList<int>> GetReleaseDueIdsAsync(DateTime asOf, CancellationToken ct = default) =>
        await context.Escrows
            .Where(e => e.Status == EscrowStatus.Held && e.ReleaseAt <= asOf)
            .Select(e => e.Id)
            .ToListAsync(ct);
}
