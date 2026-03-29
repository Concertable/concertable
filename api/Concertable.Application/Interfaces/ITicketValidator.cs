using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces;

public interface ITicketValidator
{
    Task<ValidationResult> CanPurchaseTicketAsync(int concertId, int? quantity = null);
}
