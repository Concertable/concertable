using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Application.Workflow;

internal interface IConcertPipelineFactory
{
    TStep? Create<TStep>(ContractType contractType) where TStep : class, IConcertStep;
}
