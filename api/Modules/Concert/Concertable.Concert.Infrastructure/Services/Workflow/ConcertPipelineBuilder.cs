using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class ConcertPipelineBuilder
{
    private readonly ContractType contractType;
    private readonly IServiceCollection services;
    private readonly List<ConcertStage> stages = new();

    public ConcertPipelineBuilder(ContractType contractType, IServiceCollection services)
    {
        this.contractType = contractType;
        this.services = services;
    }

    public ConcertPipelineBuilder WithSimpleApply<TStep>() where TStep : class, ISimpleApplyStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithPaidApply<TStep>() where TStep : class, IPaidApplyStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithAcceptCheckout<TStep>() where TStep : class, IAcceptCheckoutStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithApplyCheckout<TStep>() where TStep : class, IApplyCheckoutStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithSimpleAccept<TStep>() where TStep : class, ISimpleAcceptStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithPaidAccept<TStep>() where TStep : class, IPaidAcceptStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithVerify<TStep>() where TStep : class, IVerifyStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithSettle<TStep>() where TStep : class, ISettleStep => RegisterStep<TStep>();
    public ConcertPipelineBuilder WithFinish<TStep>() where TStep : class, IFinishStep => RegisterStep<TStep>();

    public ConcertPipelineBuilder WithWorkflow<TWorkflow>() where TWorkflow : class, IConcertWorkflow
    {
        services.AddKeyedScoped<IConcertWorkflow, TWorkflow>(contractType);
        return this;
    }

    public void Build()
    {
        var sequence = new[] { ConcertStage.None }.Concat(stages).ToArray();
        services.AddKeyedSingleton<IConcertTransitionValidator>(contractType, (_, _) => new ConcertTransitionValidator(sequence));
    }

    private ConcertPipelineBuilder RegisterStep<TImpl>() where TImpl : class, IConcertStep
    {
        var stage = TImpl.Stage;
        if (!stages.Contains(stage))
            stages.Add(stage);
        services.AddScoped<TImpl>();
        return this;
    }
}
