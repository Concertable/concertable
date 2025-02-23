using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
