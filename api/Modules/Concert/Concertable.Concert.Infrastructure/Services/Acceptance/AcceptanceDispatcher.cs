using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Infrastructure.Services.Acceptance;

internal class AcceptanceDispatcher : IAcceptanceDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowStrategyFactory strategyFactory;

    public AcceptanceDispatcher(IContractLoader contractLoader, IConcertWorkflowStrategyFactory strategyFactory)
    {
        this.contractLoader = contractLoader;
        this.strategyFactory = strategyFactory;
    }

    public async Task<AcceptCheckout> CheckoutAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        return await strategyFactory.Create(contract.ContractType)
            .CheckoutAsync(applicationId);
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        return await strategyFactory.Create(contract.ContractType)
            .InitiateAsync(applicationId, paymentMethodId);
    }
}
