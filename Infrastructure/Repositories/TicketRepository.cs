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

namespace Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context) { }

        public Task<byte[]> GetQrCodeByIdAsync(int id)
        {
            var query = context.Tickets
                .Where(t => t.Id == id)
                .Select(t => t.QrCode);

            return query.FirstAsync();
        }

        public async Task<IEnumerable<Ticket>> GetHistoryByUserIdAsync(int id)
        {
            var query = context.Tickets
                .Where(t => t.UserId == id && t.Event.Application.Listing.StartDate < DateTime.UtcNow)
                .Include(t => t.Event);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetUpcomingByUserIdAsync(int id)
        {
            var query = context.Tickets
                 .Where(t => t.UserId == id && t.Event.Application.Listing.StartDate >= DateTime.UtcNow)
                 .Include(t => t.Event);

            return await query.ToListAsync();
        }

        public async Task<Ticket?> GetByUserIdAndEventIdAsync(int userId, int eventId)
        {
            return await context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.EventId == eventId);
        }

    }
}
