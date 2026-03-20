using Application.Interfaces;
using Application.Interfaces.Concert;

namespace Infrastructure.Services.Accept;

public class AcceptProcessor : IAcceptProcessor
{
    private readonly IContractStrategyResolver<IAcceptStrategy> resolver;

    public AcceptProcessor(IContractStrategyResolver<IAcceptStrategy> resolver)
    {
        this.resolver = resolver;
    }

    public async Task AcceptAsync(int applicationId)
    {
        var strategy = await resolver.ResolveForApplicationAsync(applicationId);
        await strategy.AcceptAsync(applicationId);
    }
}
