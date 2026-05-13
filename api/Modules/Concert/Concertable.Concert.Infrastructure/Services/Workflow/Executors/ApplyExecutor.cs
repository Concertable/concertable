using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Capabilities;
using Concertable.Concert.Application.Workflow.Executors;
using Concertable.Shared.Exceptions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Executors;

internal class ApplyExecutor : IApplyExecutor
{
    private readonly IStepExecutor<ApplicationEntity> stepExecutor;
    private readonly IConcertWorkflowFactory workflows;
    private readonly IContractLoader contractLoader;

    public ApplyExecutor(
        IStepExecutor<ApplicationEntity> stepExecutor,
        IConcertWorkflowFactory workflows,
        IContractLoader contractLoader)
    {
        this.stepExecutor = stepExecutor;
        this.workflows = workflows;
        this.contractLoader = contractLoader;
    }

    public async Task<ApplicationEntity> ExecuteAsync(int opportunityId, int artistId, string? paymentMethodId)
    {
        var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
        var workflow = workflows.Create(contract.ContractType);

        ApplicationEntity transient = workflow switch
        {
            IAppliesPaid when paymentMethodId is { } p
                => PrepaidApplication.Create(artistId, opportunityId, contract.ContractType, p),
            IAppliesPaid
                => throw new BadRequestException("This contract requires a payment method at apply"),
            IAppliesSimple
                => StandardApplication.Create(artistId, opportunityId, contract.ContractType),
            _ => throw new BadRequestException($"Contract {workflow.Type} does not support Apply")
        };

        try
        {
            await stepExecutor.ExecuteAsync(transient, ConcertStage.Applied, Dispatch, workflow);
            return transient;
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            throw new BadRequestException("You cannot apply to the same concert opportunity twice");
        }
    }

    private static Task Dispatch(ApplicationEntity app, IConcertWorkflow workflow)
        => workflow switch
        {
            IAppliesPaid w => w.Apply.ExecuteAsync(app),
            IAppliesSimple w => w.Apply.ExecuteAsync(app),
            _ => throw new BadRequestException($"Contract {workflow.Type} does not support Apply")
        };
}
