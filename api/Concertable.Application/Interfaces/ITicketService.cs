using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Parameters;
using FluentResults;

namespace Concertable.Application.Interfaces;

public interface ITicketService
{
    Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams);
    Task<Result<TicketPaymentResponse>> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    Task<IEnumerable<TicketDto>> GetUserUpcomingAsync();
    Task<IEnumerable<TicketDto>> GetUserHistoryAsync();
}
