using Concertable.Concert.Application.Responses;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Acceptance;

internal class AcceptanceDispatcher : IAcceptanceDispatcher
{
    private readonly IContractLoader contractLoader;
    private readonly IConcertWorkflowFactory workflowFactory;

    public AcceptanceDispatcher(IContractLoader contractLoader, IConcertWorkflowFactory workflowFactory)
    {
        this.contractLoader = contractLoader;
        this.workflowFactory = workflowFactory;
    }

    public async Task<AcceptCheckout?> CheckoutAsync(int applicationId)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);
        return workflow is ICheckout co
            ? await co.CheckoutAsync(applicationId)
            : null;
    }

    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var contract = await contractLoader.LoadByApplicationIdAsync(applicationId);
        var workflow = workflowFactory.Create(contract.ContractType);

        if (paymentMethodId is not null)
            return workflow is IAcceptWithPaymentMethod withPm
                ? await withPm.AcceptAsync(applicationId, paymentMethodId)
                : throw new BadRequestException("This contract does not accept a payment method at accept");

        return workflow is IAcceptByConfirmation byConfirm
            ? await byConfirm.AcceptAsync(applicationId)
            : throw new BadRequestException("This contract requires a payment method at accept");
    }
}
