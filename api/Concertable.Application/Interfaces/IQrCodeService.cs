using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateFromTicketId(int id);
    Task<byte[]> GetByTicketIdAsync(int ticketId);
}
