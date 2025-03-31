using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<byte[]> GetQrCodeByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetUpcomingByUserIdAsync(int id);
        Task<IEnumerable<Ticket>> GetHistoryByUserIdAsync(int id);
    }
}
