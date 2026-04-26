using Concertable.Application.DTOs;
using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Application.Responses;
using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketService
{
    Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams);
    Task<Result<TicketPaymentResponse>> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    Task<IEnumerable<TicketDto>> GetUserUpcomingAsync();
    Task<IEnumerable<TicketDto>> GetUserHistoryAsync();
}
