using Application.DTOs;
using Core.Parameters;
using Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITicketService
    {
        Task<TicketPurchaseResponse> PurchaseAsync(string paymentMethodId, int eventId);
        Task<TicketPurchaseResponse> CompleteAsync(string transactionId, int eventId, int userId, string email);
        Task<byte[]> GetQrCodeByIdAsync(int id);
    }
}
