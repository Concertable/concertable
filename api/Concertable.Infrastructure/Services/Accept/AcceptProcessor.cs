using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;

namespace Concertable.Infrastructure.Services.Accept;

public class AcceptProcessor : IAcceptProcessor
{
    private readonly IContractStrategyResolver<IApplicationStrategy> resolver;

    public AcceptProcessor(IContractStrategyResolver<IApplicationStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task AcceptAsync(int applicationId)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        await strategy.AcceptAsync(applicationId);
    }
}
