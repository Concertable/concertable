using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Capabilities;
using Concertable.Concert.Application.Workflow.Executors;
using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services.Workflow.Executors;

internal class VerifyExecutor : IVerifyExecutor
{
    private readonly IStepExecutor<ApplicationEntity> stepExecutor;
    private readonly IConcertWorkflowFactory workflows;

    public VerifyExecutor(IStepExecutor<ApplicationEntity> stepExecutor, IConcertWorkflowFactory workflows)
    {
        this.stepExecutor = stepExecutor;
        this.workflows = workflows;
    }

    public Task ExecuteAsync(int applicationId)
        => stepExecutor.ExecuteAsync(applicationId, ConcertStage.Verified, Dispatch);

    private Task Dispatch(ApplicationEntity app)
    {
        var workflow = workflows.Create(app.ContractType);
        return workflow is IVerifies v
            ? v.Verify.ExecuteAsync(app.Id)
            : throw new BadRequestException($"Contract {workflow.Type} does not support Verify");
    }
}
