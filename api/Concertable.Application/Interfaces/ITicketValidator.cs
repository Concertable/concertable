using Concertable.Application.Results;

namespace Concertable.Application.Interfaces;

public interface ITicketValidator
{
    Task<ValidationResult> CanPurchaseTicketAsync(int concertId, int? quantity = null);
}
