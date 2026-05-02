namespace Concertable.Concert.Application.Interfaces;

internal interface IApplyResolver
{
    Task<ISimpleApply> ResolveSimpleAsync(int opportunityId);
    Task<IApplyWithPaymentMethod> ResolveWithPaymentMethodAsync(int opportunityId);
}
