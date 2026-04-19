using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;

namespace Concertable.Infrastructure.Services.Accept;

public class AcceptDispatcher : IAcceptDispatcher
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public AcceptDispatcher(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        return await strategy.InitiateAsync(applicationId, paymentMethodId);
    }
}
