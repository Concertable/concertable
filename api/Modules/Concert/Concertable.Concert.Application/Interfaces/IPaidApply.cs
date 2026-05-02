namespace Concertable.Concert.Application.Interfaces;

internal interface IPaidApply : IApplyable
{
    Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId, string paymentMethodId);
}
