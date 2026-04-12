using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Infrastructure.Services.Accept;

public class AcceptProcessor : IAcceptProcessor
{
    private readonly IContractStrategyResolver<IConcertWorkflowStrategy> resolver;

    public AcceptProcessor(IContractStrategyResolver<IConcertWorkflowStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task AcceptAsync(int applicationId)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        await strategy.AcceptAsync(applicationId);
    }
}
