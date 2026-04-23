using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface ITicketRepository : IGuidRepository<TicketEntity>
{
    Task<byte[]?> GetQrCodeByIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(Guid id);
    Task<TicketEntity?> GetByUserIdAndConcertIdAsync(Guid userId, int concertId);
}
