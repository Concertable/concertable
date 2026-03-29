using Concertable.Application.DTOs;
using Concertable.Core.Parameters;
using Concertable.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface ITicketService
{
    Task<TicketPurchaseResponse> PurchaseAsync(TicketPurchaseParams purchaseParams);
    Task<TicketPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    Task<IEnumerable<TicketDto>> GetUserUpcomingAsync();
    Task<IEnumerable<TicketDto>> GetUserHistoryAsync();
}
