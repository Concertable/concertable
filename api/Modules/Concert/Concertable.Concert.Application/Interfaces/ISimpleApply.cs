namespace Concertable.Concert.Application.Interfaces;

internal interface ISimpleApply : IApplyable
{
    Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId);
}
