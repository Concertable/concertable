using Concertable.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class ConcertBookingRepository : IdModuleRepository<ConcertBookingEntity, ConcertDbContext>, IConcertBookingRepository
{
    public ConcertBookingRepository(ConcertDbContext context) : base(context) { }

    public async new Task<ConcertBookingEntity?> GetByIdAsync(int id)
    {
        return await context.ConcertBookings
            .Where(b => b.Id == id)
            .Include(b => b.Application)
                .ThenInclude(a => a.Artist)
                    .ThenInclude(a => a.Genres)
            .Include(b => b.Application)
                .ThenInclude(a => a.Opportunity)
                    .ThenInclude(o => o.Venue)
            .Include(b => b.Application)
                .ThenInclude(a => a.Opportunity)
                    .ThenInclude(o => o.OpportunityGenres)
            .Include(b => b.Concert)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertBookingEntity?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.ConcertBookings
            .Where(b => b.ApplicationId == applicationId)
            .Include(b => b.Application)
                .ThenInclude(a => a.Opportunity)
            .Include(b => b.Concert)
            .FirstOrDefaultAsync();
    }

    public async Task<ConcertBookingEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.ConcertBookings
            .Where(b => b.Concert!.Id == concertId)
            .Include(b => b.Application)
                .ThenInclude(a => a.Artist)
            .Include(b => b.Application)
                .ThenInclude(a => a.Opportunity)
                    .ThenInclude(o => o.Venue)
            .Include(b => b.Concert)
            .FirstOrDefaultAsync();
    }
}
