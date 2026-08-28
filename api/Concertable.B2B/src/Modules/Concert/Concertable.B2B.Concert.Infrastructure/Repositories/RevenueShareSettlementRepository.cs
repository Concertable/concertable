using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.B2B.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.B2B.Concert.Infrastructure.Repositories;

internal sealed class RevenueShareSettlementRepository
    : Repository<RevenueShareSettlementEntity>, IRevenueShareSettlementRepository
{
    private readonly ConcertDbContext context;

    public RevenueShareSettlementRepository(ConcertDbContext context) : base(context)
    {
        this.context = context;
    }

    public Task<RevenueShareSettlementEntity?> GetByConcertIdAsync(int concertId, CancellationToken ct = default) =>
        context.RevenueShareSettlements.FirstOrDefaultAsync(s => s.ConcertId == concertId, ct);
}
