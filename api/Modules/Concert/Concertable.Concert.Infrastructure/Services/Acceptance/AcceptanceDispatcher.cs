using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Infrastructure.Services.Acceptance;

internal class AcceptanceDispatcher : IAcceptanceDispatcher
{
    private readonly IContractLookup contractLookup;
    private readonly IConcertWorkflowStrategyFactory strategyFactory;

    public AcceptanceDispatcher(IContractLookup contractLookup, IConcertWorkflowStrategyFactory strategyFactory)
    {
        this.contractLookup = contractLookup;
        this.strategyFactory = strategyFactory;
    }

    public async Task<AcceptCheckout> CheckoutAsync(int applicationId)
    {
        var contract = await contractLookup.GetByApplicationIdAsync(applicationId);
        return await strategyFactory.Create(contract.ContractType)
            .CheckoutAsync(applicationId);
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractLookup.GetByApplicationIdAsync(applicationId);
        return await strategyFactory.Create(contract.ContractType)
            .InitiateAsync(applicationId, paymentMethodId);
    }
}
