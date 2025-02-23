using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateTicketReciptAsync(string email, int ticketId);
    }
}
