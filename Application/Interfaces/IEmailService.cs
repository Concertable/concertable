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
        Task SendTicketsToEmailAsync(string toEmail, IEnumerable<int> ticketIds);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
