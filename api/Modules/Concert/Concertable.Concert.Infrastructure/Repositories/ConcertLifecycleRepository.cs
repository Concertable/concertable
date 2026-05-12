using Concertable.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class ConcertLifecycleRepository : Repository<ConcertLifecycleEntity>, IConcertLifecycleRepository
{
    public ConcertLifecycleRepository(ConcertDbContext context) : base(context) { }

    public Task<int?> GetIdByOpportunityIdAndArtistIdAsync(int opportunityId, int artistId) =>
        context.ConcertLifecycles
            .Where(l => l.OpportunityId == opportunityId && l.ArtistId == artistId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync();

    public Task<int?> GetIdByApplicationIdAsync(int applicationId) =>
        context.Applications
            .Where(a => a.Id == applicationId)
            .Select(a => (int?)a.LifecycleId)
            .FirstOrDefaultAsync();

    public Task<int?> GetIdByBookingIdAsync(int bookingId) =>
        context.Bookings
            .Where(b => b.Id == bookingId)
            .Select(b => (int?)b.Application.LifecycleId)
            .FirstOrDefaultAsync();

    public Task<int?> GetIdByConcertIdAsync(int concertId) =>
        context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (int?)c.Booking.Application.LifecycleId)
            .FirstOrDefaultAsync();

    public Task<int?> GetApplicationIdByLifecycleIdAsync(int lifecycleId) =>
        context.Applications
            .Where(a => a.LifecycleId == lifecycleId)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();

    public Task<int?> GetBookingIdByLifecycleIdAsync(int lifecycleId) =>
        context.Bookings
            .Where(b => b.Application.LifecycleId == lifecycleId)
            .Select(b => (int?)b.Id)
            .FirstOrDefaultAsync();

    public Task<int?> GetConcertIdByLifecycleIdAsync(int lifecycleId) =>
        context.Concerts
            .Where(c => c.Booking.Application.LifecycleId == lifecycleId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
}
