using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    private readonly TimeProvider timeProvider;

    public TicketRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public Task<byte[]?> GetQrCodeByIdAsync(int id)
    {
        var query = context.Tickets
            .Where(t => t.Id == id)
            .Select(t => t.QrCode);

        return query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Ticket>> GetHistoryByUserIdAsync(int id)
    {
        var query = context.Tickets
            .Where(t => t.UserId == id && t.Concert.Application.Listing.StartDate < timeProvider.GetUtcNow())
            .Include(t => t.Concert);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetUpcomingByUserIdAsync(int id)
    {
        var query = context.Tickets
             .Where(t => t.UserId == id && t.Concert.Application.Listing.StartDate >= timeProvider.GetUtcNow())
             .Include(t => t.Concert);

        return await query.ToListAsync();
    }

    public async Task<Ticket?> GetByUserIdAndConcertIdAsync(int userId, int concertId)
    {
        return await context.Tickets
            .Include(t => t.Concert)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ConcertId == concertId);
    }

}
