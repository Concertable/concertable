using Concertable.Application.Interfaces;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Repositories;

internal class TicketRepository : GuidModuleRepository<TicketEntity, ConcertDbContext>, ITicketRepository
{
    private readonly TimeProvider timeProvider;

    public TicketRepository(ConcertDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public Task<byte[]?> GetQrCodeByIdAsync(Guid id)
    {
        return context.Tickets
            .Where(t => t.Id == id)
            .Select(t => t.QrCode)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(Guid id)
    {
        return await context.Tickets
            .Where(t => t.UserId == id && t.Concert.Booking.Application.Opportunity.Period.Start < timeProvider.GetUtcNow())
            .Include(t => t.Concert).ThenInclude(c => c.Booking).ThenInclude(b => b.Application).ThenInclude(a => a.Opportunity).ThenInclude(o => o.Venue)
            .Include(t => t.Concert).ThenInclude(c => c.Booking).ThenInclude(b => b.Application).ThenInclude(a => a.Artist)
            .ToListAsync();
    }

    public async Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(Guid id)
    {
        return await context.Tickets
            .Where(t => t.UserId == id && t.Concert.Booking.Application.Opportunity.Period.Start >= timeProvider.GetUtcNow())
            .Include(t => t.Concert).ThenInclude(c => c.Booking).ThenInclude(b => b.Application).ThenInclude(a => a.Opportunity).ThenInclude(o => o.Venue)
            .Include(t => t.Concert).ThenInclude(c => c.Booking).ThenInclude(b => b.Application).ThenInclude(a => a.Artist)
            .ToListAsync();
    }

    public async Task<TicketEntity?> GetByUserIdAndConcertIdAsync(Guid userId, int concertId)
    {
        return await context.Tickets
            .Include(t => t.Concert)
                .ThenInclude(c => c.Booking)
                    .ThenInclude(b => b.Application)
                        .ThenInclude(a => a.Opportunity)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ConcertId == concertId);
    }
}
