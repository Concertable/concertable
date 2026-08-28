using Concertable.B2B.Concert.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.B2B.Concert.Application.Interfaces;

internal interface IRevenueShareSettlementRepository : IRepository<RevenueShareSettlementEntity>
{
    /// <summary>The revenue-share settlement record for a concert, or null if the venue has not declared the door take.</summary>
    Task<RevenueShareSettlementEntity?> GetByConcertIdAsync(int concertId, CancellationToken ct = default);
}
