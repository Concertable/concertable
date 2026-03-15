using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ITicketRepository : IRepository<TicketEntity>
{
    Task<byte[]?> GetQrCodeByIdAsync(int id);
    Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(int id);
    Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(int id);
    Task<TicketEntity?> GetByUserIdAndConcertIdAsync(int userId, int concertId);
}
