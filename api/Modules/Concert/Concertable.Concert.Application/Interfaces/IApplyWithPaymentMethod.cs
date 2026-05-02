namespace Concertable.Concert.Application.Interfaces;

internal interface IApplyWithPaymentMethod
{
    Task OnAppliedAsync(int applicationId, string paymentMethodId);
}
