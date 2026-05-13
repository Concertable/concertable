using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class ConcertPipelineBuilder
{
    private readonly ContractType contractType;
    private readonly IServiceCollection services;

    public ConcertPipelineBuilder(ContractType contractType, IServiceCollection services)
    {
        this.contractType = contractType;
        this.services = services;
    }

    public ConcertPipelineBuilder WithSimpleApply<TStep>() where TStep : class, ISimpleApplyStep => Register<TStep>();
    public ConcertPipelineBuilder WithPaidApply<TStep>() where TStep : class, IPaidApplyStep => Register<TStep>();
    public ConcertPipelineBuilder WithAcceptCheckout<TStep>() where TStep : class, IAcceptCheckoutStep => Register<TStep>();
    public ConcertPipelineBuilder WithApplyCheckout<TStep>() where TStep : class, IApplyCheckoutStep => Register<TStep>();
    public ConcertPipelineBuilder WithSimpleAccept<TStep>() where TStep : class, ISimpleAcceptStep => Register<TStep>();
    public ConcertPipelineBuilder WithPaidAccept<TStep>() where TStep : class, IPaidAcceptStep => Register<TStep>();
    public ConcertPipelineBuilder WithVerify<TStep>() where TStep : class, IVerifyStep => Register<TStep>();
    public ConcertPipelineBuilder WithSettle<TStep>() where TStep : class, ISettleStep => Register<TStep>();
    public ConcertPipelineBuilder WithFinish<TStep>() where TStep : class, IFinishStep => Register<TStep>();

    public ConcertPipelineBuilder WithStateMachine<TStateMachine>() where TStateMachine : class, IConcertStateMachine
    {
        services.AddKeyedSingleton<IConcertStateMachine, TStateMachine>(contractType);
        return this;
    }

    public ConcertPipelineBuilder WithWorkflow<TWorkflow>() where TWorkflow : class, IConcertWorkflow
    {
        services.AddKeyedScoped<IConcertWorkflow, TWorkflow>(contractType);
        return this;
    }

    private ConcertPipelineBuilder Register<TImpl>() where TImpl : class
    {
        services.AddScoped<TImpl>();
        return this;
    }
}
