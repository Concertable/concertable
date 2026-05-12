using Concertable.Concert.Application.Workflow;
using Concertable.Concert.Application.Workflow.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Concert.Infrastructure.Services.Workflow;

internal class ConcertPipelineBuilder
{
    private readonly ContractType contractType;
    private readonly IServiceCollection services;
    private readonly ConcertPipelineRegistry registry;

    public ConcertPipelineBuilder(ContractType contractType, IServiceCollection services, ConcertPipelineRegistry registry)
    {
        this.contractType = contractType;
        this.services = services;
        this.registry = registry;
    }

    public ConcertPipelineBuilder WithSimpleApply<TStep>() where TStep : class, ISimpleApplyStep => Register<ISimpleApplyStep, TStep>();
    public ConcertPipelineBuilder WithPaidApply<TStep>() where TStep : class, IPaidApplyStep => Register<IPaidApplyStep, TStep>();
    public ConcertPipelineBuilder WithAcceptCheckout<TStep>() where TStep : class, IAcceptCheckoutStep => Register<IAcceptCheckoutStep, TStep>();
    public ConcertPipelineBuilder WithApplyCheckout<TStep>() where TStep : class, IApplyCheckoutStep => Register<IApplyCheckoutStep, TStep>();
    public ConcertPipelineBuilder WithSimpleAccept<TStep>() where TStep : class, ISimpleAcceptStep => Register<ISimpleAcceptStep, TStep>();
    public ConcertPipelineBuilder WithPaidAccept<TStep>() where TStep : class, IPaidAcceptStep => Register<IPaidAcceptStep, TStep>();
    public ConcertPipelineBuilder WithVerify<TStep>() where TStep : class, IVerifyStep => Register<IVerifyStep, TStep>();
    public ConcertPipelineBuilder WithSettle<TStep>() where TStep : class, ISettleStep => Register<ISettleStep, TStep>();
    public ConcertPipelineBuilder WithFinish<TStep>() where TStep : class, IFinishStep => Register<IFinishStep, TStep>();

    public ConcertPipelineBuilder WithStateMachine<TStateMachine>() where TStateMachine : class, IConcertStateMachine
    {
        services.AddKeyedScoped<IConcertStateMachine, TStateMachine>(contractType);
        return this;
    }

    private ConcertPipelineBuilder Register<TInterface, TImpl>()
        where TInterface : class, IConcertStep
        where TImpl : class, TInterface
    {
        services.AddKeyedScoped<TInterface, TImpl>(contractType);
        registry.Register(contractType, typeof(TInterface));
        return this;
    }
}
