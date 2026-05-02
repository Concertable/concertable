namespace Concertable.Concert.Application.Interfaces;

internal interface IApplyWithPaymentMethod : IApplyable
{
    Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId, string paymentMethodId);
}
