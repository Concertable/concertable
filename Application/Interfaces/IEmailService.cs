using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task SendTicketsToEmailAsync(int userId, string toEmail, IEnumerable<int> ticketIds);
        Task SendEmailAsync(int userId, string toEmail, string subject, string body);
    }
}
