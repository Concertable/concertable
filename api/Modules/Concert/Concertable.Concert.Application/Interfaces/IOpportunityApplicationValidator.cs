using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface IOpportunityApplicationValidator
{
    Task<Result> CanApplyAsync(int opportunityId, int artistId);
    Task<Result> CanAcceptAsync(int applicationId);
}
