using Application.Responses;

namespace Application.Interfaces;

public interface ITicketValidator
{
    Task<ValidationResult> CanPurchaseTicketAsync(int concertId, int? quantity = null);
}
