using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Capabilities;
using Concertable.Concert.Application.Workflow.Executors;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Executors;

internal class AcceptExecutor : IAcceptExecutor
{
    private readonly IStepExecutor<ApplicationEntity> stepExecutor;
    private readonly IConcertWorkflowFactory workflows;

    public AcceptExecutor(IStepExecutor<ApplicationEntity> stepExecutor, IConcertWorkflowFactory workflows)
    {
        this.stepExecutor = stepExecutor;
        this.workflows = workflows;
    }

    public Task ExecuteAsync(int applicationId, string? paymentMethodId)
        => stepExecutor.ExecuteAsync(applicationId, ConcertStage.Accepted, Dispatch, paymentMethodId);

    private Task Dispatch(ApplicationEntity app, string? pmId)
    {
        var workflow = workflows.Create(app.ContractType);
        return workflow switch
        {
            IAcceptsPaid w when pmId is { } p => w.Accept.ExecuteAsync(app.Id, p),
            IAcceptsPaid => throw new BadRequestException("This contract requires a payment method at acceptance"),
            IAcceptsSimple w => w.Accept.ExecuteAsync(app.Id),
            _ => throw new BadRequestException($"Contract {workflow.Type} does not support Accept")
        };
    }
}
