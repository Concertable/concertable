using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Capabilities;
using Concertable.Concert.Application.Workflow.Executors;
using Concertable.Shared.Exceptions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Executors;

internal class ApplyExecutor : IApplyExecutor
{
    private readonly IWorkflowStateMachine<ApplicationEntity> stateMachine;
    private readonly IConcertWorkflowFactory workflows;
    private readonly IContractLoader contractLoader;

    public ApplyExecutor(
        IWorkflowStateMachine<ApplicationEntity> stateMachine,
        IConcertWorkflowFactory workflows,
        IContractLoader contractLoader)
    {
        this.stateMachine = stateMachine;
        this.workflows = workflows;
        this.contractLoader = contractLoader;
    }

    public async Task<ApplicationEntity> ExecuteAsync(int opportunityId, int artistId, string? paymentMethodId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var workflow = workflows.Create(contract.ContractType);

        ApplicationEntity transient = workflow switch
        {
            IAppliesPaid w when paymentMethodId is { } p
                => await w.Apply.ApplyAsync(artistId, opportunityId, contract.ContractType, p),
            IAppliesPaid
                => throw new BadRequestException("This contract requires a payment method at apply"),
            IAppliesSimple w
                => await w.Apply.ApplyAsync(artistId, opportunityId, contract.ContractType),
            _ => throw new BadRequestException($"Contract {workflow.Type} does not support Apply")
        };

        try
        {
            await stateMachine.TransitionAsync(transient, ConcertStage.Applied);
            return transient;
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            throw new BadRequestException("You cannot apply to the same concert opportunity twice");
        }
    }
}
