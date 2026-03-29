using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface ITicketRepository : IRepository<TicketEntity>
{
    Task<byte[]?> GetQrCodeByIdAsync(int id);
    Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(Guid id);
    Task<TicketEntity?> GetByUserIdAndConcertIdAsync(Guid userId, int concertId);
}
