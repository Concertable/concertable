using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class ConcertWorkflowBuilder
{
    private readonly ContractType contractType;
    private readonly IServiceCollection services;
    private readonly List<ConcertStage> stages = [];
    private Type workflowType = null!;

    public ConcertWorkflowBuilder(ContractType contractType, IServiceCollection services)
    {
        this.contractType = contractType;
        this.services = services;
    }

    public ConcertWorkflowBuilder WithSimpleApply<TStep>() where TStep : class, ISimpleApplyStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithPaidApply<TStep>() where TStep : class, IPaidApplyStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithAcceptCheckout<TStep>() where TStep : class, IAcceptCheckoutStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithApplyCheckout<TStep>() where TStep : class, IApplyCheckoutStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithSimpleAccept<TStep>() where TStep : class, ISimpleAcceptStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithPaidAccept<TStep>() where TStep : class, IPaidAcceptStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithVerify<TStep>() where TStep : class, IVerifyStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithSettle<TStep>() where TStep : class, ISettleStep => RegisterStep<TStep>();
    public ConcertWorkflowBuilder WithFinish<TStep>() where TStep : class, IFinishStep => RegisterStep<TStep>();

    public ConcertWorkflowBuilder WithWorkflow<TWorkflow>() where TWorkflow : class, IConcertWorkflow
    {
        services.AddKeyedScoped<IConcertWorkflow, TWorkflow>(contractType);
        workflowType = typeof(TWorkflow);
        return this;
    }

    public Type Build()
    {
        if (workflowType is null)
            throw new InvalidOperationException($"No workflow registered for {contractType}. Call WithWorkflow<T>().");
        var sequence = new[] { ConcertStage.None }.Concat(stages).ToArray();
        services.AddKeyedSingleton<IConcertTransitionValidator>(contractType, (_, _) => new ConcertTransitionValidator(sequence));
        return workflowType;
    }

    private ConcertWorkflowBuilder RegisterStep<TStep>() where TStep : class, IConcertStep
    {
        var stage = TStep.Stage;
        if (!stages.Contains(stage))
            stages.Add(stage);
        services.AddScoped<TStep>();
        return this;
    }
}
