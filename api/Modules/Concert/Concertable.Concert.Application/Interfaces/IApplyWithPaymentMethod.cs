namespace Concertable.Concert.Application.Interfaces;

internal interface IApplyWithPaymentMethod : IApplyable
{
    Task OnAppliedAsync(int applicationId, string paymentMethodId);
}
