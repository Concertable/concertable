using Concertable.Concert.Application.Workflow.Steps;

namespace Concertable.Concert.Application.Workflow;

internal interface IConcertPipelineRegistry
{
    bool Has<TStep>(ContractType contractType) where TStep : IConcertStep;
}
