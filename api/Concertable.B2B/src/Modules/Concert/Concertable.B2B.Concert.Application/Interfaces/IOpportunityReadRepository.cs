using Concertable.B2B.Concert.Application.Projections;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.Contracts;

namespace Concertable.B2B.Concert.Application.Interfaces;

/// <summary>
/// Read-only projections over opportunities, run on the read-only <c>ConcertReadDbContext</c>. That
/// context composes no tenant filters, so the open/active check sees <b>all</b> parties' applications —
/// an opportunity already booked by another tenant correctly stops showing as open — and a caller
/// wanting one tenant's opportunities passes that tenant key explicitly. Covers both the anonymous
/// marketplace browse and a venue's own dashboard projections. Tracked reads that feed a mutation
/// live on <see cref="IOpportunityRepository"/>.
/// </summary>
internal interface IOpportunityReadRepository
{
    Task<IPagination<OpportunityEntity>> GetActiveByVenueIdAsync(int venueId, IPageParams pageParams);
    Task<IEnumerable<OpportunityEntity>> GetActiveByVenueIdAsync(int venueId);
    Task<IReadOnlyList<OpportunityMatchProjection>> GetMatchCandidatesAsync(
        int artistId,
        IReadOnlySet<Genre> genres);
    Task<IReadOnlyList<OpportunityApplicationProjection>> GetOpenWithApplicationCountsByVenueTenantIdAsync(
        Guid venueTenantId);
}
